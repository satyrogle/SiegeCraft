"""Duel wins per class from tune/out CSVs. usage: python -I score.py file.csv [file.csv ...]
Each line: seed,build,n,kind,tribe,won,ticks,livesAfter,... (RunSweep.FightLine)."""
import sys
from collections import defaultdict

CLASSES = ['Marksman', 'Skirmisher', 'Warden', 'Trapper', 'Medic']


def duel_wins(path):
    won, count, ticks = defaultdict(int), defaultdict(int), defaultdict(int)
    for line in open(path):
        p = line.strip().split(',')
        if len(p) != 15 or p[3] != 'duel': continue
        won[p[1]] += int(p[5]); count[p[1]] += 1; ticks[p[1]] += int(p[6])
    return {c: (100 * won['plain.' + c] / max(1, count['plain.' + c]), count['plain.' + c],
                ticks['plain.' + c] / max(1, count['plain.' + c]) / 30) for c in CLASSES}


if __name__ == '__main__':
    for path in sys.argv[1:]:
        r = duel_wins(path)
        mean = sum(v[0] for v in r.values()) / len(r)
        name = path.replace('\\', '/').split('/')[-1][:-4]
        print(f'{name:28s} mean {mean:5.1f} | ' + '  '.join(f'{c[:4]} {v[0]:5.1f}' for c, v in r.items())
              + f' | duels {min(v[1] for v in r.values())}-{max(v[1] for v in r.values())}'
              + f' | {min(v[2] for v in r.values()):.0f}-{max(v[2] for v in r.values()):.0f} s')
