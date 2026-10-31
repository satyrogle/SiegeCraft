"""Reads the sweep's CSVs (seed, fight, kind, build, won, margin permille, ticks, combo events) and prints, for each combo,
how much it changes the win chance and the margin against the plain squad on the very same fights (paired), with standard
errors. With 'pairs' it also prints each pair's synergy: what the two do together beyond what each does alone.
Usage: analyze.py singles <csv> [<csv> ...] | pairs <csv> | builds <csv>"""
import math
import sys
from collections import defaultdict

ORDER = ['BlackHoleRound', 'Collapse', 'EventHorizon', 'SingularityNet', 'MercyWell',
         'AroundTheCorner', 'ExecutionCorridor', 'WrongDoor', 'Doorway', 'Evac',
         'StoppedBullets', 'FrozenFlurry', 'StillWall', 'TimeSnare', 'Rewind']
CLASS = ['Marksman', 'Skirmisher', 'Warden', 'Trapper', 'Medic']
AUG = ['Gravity Well', 'Fold Door', 'Time Stop']


def load(path):
    fights = defaultdict(dict)
    for line in open(path, encoding='utf-8'):
        p = line.strip().split(',')
        if len(p) != 8:
            continue
        seed, n, kind, build = p[0], p[1], p[2], p[3]
        fights[(seed, n)][build] = (int(p[4]), int(p[5]), int(p[6]), int(p[7]), kind)
    return fights


def mean_se(xs):
    n = len(xs)
    if n == 0:
        return 0.0, 0.0
    m = sum(xs) / n
    if n < 2:
        return m, 0.0
    v = sum((x - m) ** 2 for x in xs) / (n - 1)
    return m, math.sqrt(v / n)


def singles(path):
    fights = load(path)
    plain = [f['plain'] for f in fights.values() if 'plain' in f]
    pw, _ = mean_se([p[0] for p in plain])
    pm, _ = mean_se([p[1] for p in plain])
    print('%s: %d fights, plain squad wins %.0f%%, margin %+.0f' % (path.split('/')[-1].split('\\')[-1], len(plain), 100 * pw, pm))
    rows = []
    for i, c in enumerate(ORDER):
        dw, dm, fx, fired = [], [], [], 0
        for f in fights.values():
            if 'plain' not in f or c not in f:
                continue
            dw.append(f[c][0] - f['plain'][0])
            dm.append(f[c][1] - f['plain'][1])
            fx.append(f[c][3])
            fired += f[c][3] > 0
        w, wse = mean_se(dw)
        m, mse = mean_se(dm)
        rows.append((c, AUG[i // 5], CLASS[i % 5], 100 * w, 100 * wse, m, mse, sum(fx) / max(1, len(fx)), fired / max(1, len(fx)), len(dw)))
    print('  %-18s %-12s %-10s %14s %14s %8s %6s' % ('combo', 'augment', 'class', 'win pts', 'margin', 'fires', 'fights'))
    for r in sorted(rows, key=lambda r: -r[5]):
        print('  %-18s %-12s %-10s %+6.1f (%4.1f) %+6.0f (%4.0f) %8.1f %6d' % (r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7], r[9]))
    return rows


def pairs(path):
    fights = load(path)
    out = []
    for i, a in enumerate(ORDER):
        for j, b in enumerate(ORDER):
            if j <= i or i % 5 == j % 5:
                continue
            key = a + '+' + b
            sw, sm, bw, bm = [], [], [], []
            for f in fights.values():
                if not all(k in f for k in ('plain', a, b, key)):
                    continue
                p, x, y, xy = f['plain'], f[a], f[b], f[key]
                sw.append(xy[0] - x[0] - y[0] + p[0])
                sm.append(xy[1] - x[1] - y[1] + p[1])
                bw.append(xy[0] - p[0])
                bm.append(xy[1] - p[1])
            w, wse = mean_se(sw)
            m, mse = mean_se(sm)
            tw, _ = mean_se(bw)
            tm, _ = mean_se(bm)
            out.append((a, b, 100 * tw, tm, 100 * w, 100 * wse, m, mse, len(sw)))
    print('%s: %d pairs. Synergy = together minus each alone (paired, same fights).' % (path.split('/')[-1].split('\\')[-1], len(out)))
    big = sum(1 for r in out if abs(r[6]) > 2.5 * max(r[7], 1e-9))
    print('  pairs whose margin synergy is more than 2.5 standard errors from zero: %d of %d' % (big, len(out)))
    print('  mean |margin synergy| %.0f, mean |win synergy| %.1f pts' % (
        sum(abs(r[6]) for r in out) / len(out), sum(abs(r[4]) for r in out) / len(out)))
    print('  %-36s %12s %12s %16s %14s' % ('pair', 'both win', 'both margin', 'synergy win', 'synergy margin'))
    ranked = sorted(out, key=lambda r: -r[6])
    for r in ranked[:10] + [None] + ranked[-10:]:
        if r is None:
            print('  ...')
            continue
        print('  %-36s %+11.1f %+12.0f %+9.1f (%4.1f) %+8.0f (%3.0f)' % (r[0] + ' + ' + r[1], r[2], r[3], r[4], r[5], r[6], r[7]))
    return out


def builds(path):
    """Ordinary least squares on random whole builds: margin and win against the plain squad on the same fight, explained
    by which combos the build wears. R squared says how much a sum of single combo effects explains."""
    fights = load(path)
    xs, ym, yw = [], [], []
    for f in fights.values():
        if 'plain' not in f:
            continue
        for name, v in f.items():
            if name == 'plain' or name in ORDER and False:
                continue
            combos = name.split('+') if name else []
            x = [1.0] + [1.0 if c in combos else 0.0 for c in ORDER]
            xs.append(x)
            ym.append(v[1] - f['plain'][1])
            yw.append(v[0] - f['plain'][0])
    k = len(ORDER) + 1

    def ols(y):
        a = [[sum(xs[r][i] * xs[r][j] for r in range(len(xs))) for j in range(k)] for i in range(k)]
        b = [sum(xs[r][i] * y[r] for r in range(len(xs))) for i in range(k)]
        # Gaussian elimination.
        for i in range(k):
            piv = max(range(i, k), key=lambda r: abs(a[r][i]))
            a[i], a[piv] = a[piv], a[i]
            b[i], b[piv] = b[piv], b[i]
            for r in range(i + 1, k):
                f = a[r][i] / a[i][i]
                for c in range(i, k):
                    a[r][c] -= f * a[i][c]
                b[r] -= f * b[i]
        beta = [0.0] * k
        for i in range(k - 1, -1, -1):
            beta[i] = (b[i] - sum(a[i][c] * beta[c] for c in range(i + 1, k))) / a[i][i]
        pred = [sum(beta[i] * x[i] for i in range(k)) for x in xs]
        mean = sum(y) / len(y)
        ss = sum((v - mean) ** 2 for v in y)
        res = sum((v - p) ** 2 for v, p in zip(y, pred))
        return beta, 1 - res / ss if ss > 0 else 0.0

    bm, r2m = ols(ym)
    bw, r2w = ols(yw)
    print('%s: %d random builds over %d fights. OLS, each combo\'s effect when worn with others:' % (
        path.split('/')[-1].split('\\')[-1], len(xs), len(fights)))
    print('  R squared: margin %.2f, win %.2f (how much the sum of single effects explains)' % (r2m, r2w))
    print('  intercept: margin %+.0f, win %+.1f pts' % (bm[0], 100 * bw[0]))
    for i, c in sorted(enumerate(ORDER), key=lambda t: -bm[t[0] + 1]):
        print('  %-18s margin %+5.0f   win %+5.1f pts' % (c, bm[i + 1], 100 * bw[i + 1]))


if __name__ == '__main__':
    mode = sys.argv[1]
    for p in sys.argv[2:]:
        {'singles': singles, 'pairs': pairs, 'builds': builds}[mode](p)
        print()
