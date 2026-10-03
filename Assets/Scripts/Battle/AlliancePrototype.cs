using System.Collections.Generic;
using UnityEngine;
using KindredSiege.AI.BehaviourTree;
using KindredSiege.Core;

namespace KindredSiege.Battle
{
    // An opt-in encounter harness, not a replacement for BattleManager or the deterministic core.
    // Uses existing UnitController damage, cooldowns, sanity and BT execution with runtime-only UnitData.
    public sealed class AlliancePrototype : MonoBehaviour
    {
        public enum Stage { Briefing, FirstFight, Debrief, SecondFight, Complete, Defeat }
        public enum Order { Wait, Rescue, Salvage, Advance, CoverGuards }
        // A material asset under Resources keeps its Unlit/Color shader inside player builds; Shader.Find alone did not.
        public const string UnlitMaterialPath = "AlliancePrototype/AlliancePrototypeUnlit";
        public Stage CurrentStage { get; private set; }
        public Order CurrentOrder { get; private set; }
        public AllianceMemory Memory { get; private set; } = new AllianceMemory();
        public bool Paused { get; private set; }
        public float Elapsed { get; private set; }
        public int Charges { get; private set; }
        public int CoverCount { get; private set; }
        public int PulseCount { get; private set; }
        public bool PulsePending => _pulseTarget != null && _pulseTarget.IsAlive;
        public bool PersistenceEnabled { get; set; } = true;
        public string Status { get; private set; } = "Two rivals. One way out.";
        public Camera ViewCamera { get; private set; }
        public IReadOnlyList<UnitController> Fighters => _all;

        readonly List<UnitController> _all = new List<UnitController>();
        readonly List<UnitController> _friends = new List<UnitController>();
        readonly List<UnitController> _enemies = new List<UnitController>();
        readonly List<Object> _owned = new List<Object>();
        readonly List<Object> _actorOwned = new List<Object>();
        readonly HashSet<int> _fallen = new HashSet<int>();
        readonly List<Rect> _tagRects = new List<Rect>();
        readonly Vector3 _guardPoint = new Vector3(-2, 0.8f, 3);
        readonly Vector3 _salvagePoint = new Vector3(5, 0.8f, -3);
        UnitController _commander, _scout, _captor, _boss, _pulseTarget;
        BattleGrid _grid;
        Transform _actors;
        GameObject _crate, _pulseMarker;
        Material _unlitBase;
        float _collection, _nextPulse, _pulseAt;
        int _nextId, _pulseSequence, _guardTurn;
        GUIStyle _title, _body, _small, _button;
        Vector2 _historyScroll;
        string _storageMessage = "Prototype progress is separate from your campaign.";
        readonly Color _amber = new Color(1f, .69f, .3f);
        readonly Color _blue = new Color(.35f, .82f, .91f);
        readonly Color _violet = new Color(.85f, .36f, .85f);

        void Start()
        {
            if (GameManager.Instance != null || BattleManager.Instance != null)
            {
                Debug.LogError("Alliance prototype must be opened in its own scene, outside the campaign.");
                enabled = false;
                return;
            }
            _unlitBase = Resources.Load<Material>(UnlitMaterialPath);
            if (_unlitBase == null)
            {
                Debug.LogError("Alliance prototype is missing its material at Resources/" + UnlitMaterialPath + ".mat.");
                enabled = false;
                return;
            }
            BuildArena();
            SpawnFirstFight();
        }

        // BattleManager normally applies the witness sanity rules; this scene leaves it out, so they live here.
        void OnEnable()
        {
            EventBus.Subscribe<UnitDefeatedEvent>(OnUnitDefeated);
            EventBus.Subscribe<UnitLostEvent>(OnUnitLost);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<UnitDefeatedEvent>(OnUnitDefeated);
            EventBus.Unsubscribe<UnitLostEvent>(OnUnitLost);
        }

        public void BeginFirstFight()
        {
            if (CurrentStage != Stage.Briefing) return;
            CurrentStage = Stage.FirstFight;
            CurrentOrder = Order.Wait;
            Status = "The Ash guards are pinned. Choose where your two fighters go.";
        }

        public bool IssueOrder(Order order)
        {
            if (Paused) return false;
            if (CurrentStage == Stage.FirstFight && (order == Order.Rescue || order == Order.Salvage) ||
                CurrentStage == Stage.SecondFight && (order == Order.Advance || order == Order.CoverGuards))
            {
                // Repeating the current order keeps extraction progress; only a new order restarts it.
                if (order != CurrentOrder) _collection = 0;
                CurrentOrder = order;
                return true;
            }
            return false;
        }

        public void TogglePause()
        {
            if (CurrentStage == Stage.FirstFight || CurrentStage == Stage.SecondFight) Paused = !Paused;
        }

        public bool DisruptPulse()
        {
            if (Paused || CurrentStage != Stage.SecondFight || Charges < 1 || !PulsePending) return false;
            Charges--;
            _pulseTarget = null;
            Status = "Disruptor fired. The pulse failed. " + Charges + " charge(s) left.";
            return true;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) TogglePause();
            if (Input.GetKeyDown(KeyCode.Alpha1)) IssueOrder(CurrentStage == Stage.FirstFight ? Order.Rescue : Order.Advance);
            if (Input.GetKeyDown(KeyCode.Alpha2)) IssueOrder(CurrentStage == Stage.FirstFight ? Order.Salvage : Order.CoverGuards);
            if (Input.GetKeyDown(KeyCode.Alpha3)) DisruptPulse();
            if (Paused || (CurrentStage != Stage.FirstFight && CurrentStage != Stage.SecondFight)) return;

            Elapsed += Time.deltaTime;
            foreach (var unit in _all) if (unit.IsAlive) unit.TickAI();
            ObserveCasualties();
            if (!_commander.IsAlive) { Finish(false, "Your commander fell. This fight is lost."); return; }
            if (CurrentStage == Stage.FirstFight) UpdateFirstFight();
            else UpdateSecondFight();
        }

        void UpdateFirstFight()
        {
            UnitController rescuer = AssistedRescue(_commander) ? _commander : AssistedRescue(_scout) ? _scout : null;
            if (!_captor.IsAlive && rescuer != null && CurrentOrder == Order.Rescue)
            {
                foreach (var unit in _friends)
                    if (IsGuard(unit) && unit.IsAlive)
                        Memory.Record(1, (int)Elapsed, unit.UnitName, AllianceFact.Rescued, rescuer.UnitName);
                EndFirstFight("You broke the captor's hold. The salvage sank into the breach.");
                return;
            }
            if (CurrentOrder == Order.Salvage && (Near(_commander, _salvagePoint, 1.8f) || Near(_scout, _salvagePoint, 1.8f)))
            {
                _collection += Time.deltaTime;
                if (_collection >= 4)
                {
                    Memory.HasSalvage = true;
                    Memory.Record(1, (int)Elapsed, "Commander", AllianceFact.Salvaged);
                    EndFirstFight("You recovered the disruptor. The surviving guards escaped without your help.");
                    return;
                }
            }
            else _collection = 0;
            if (Elapsed >= 22) EndFirstFight("The breach closed. You recovered no salvage and secured no rescue.");
        }

        void EndFirstFight(string reason)
        {
            foreach (var unit in _friends)
            {
                if (IsGuard(unit) && unit.IsAlive && !Memory.WillCover(unit.UnitName))
                    Memory.Record(1, (int)Elapsed, unit.UnitName, AllianceFact.Abandoned);
                Memory.Survivors.Add(new AllianceSurvivor { Id = unit.UnitName,
                    Health = unit.IsAlive ? unit.CurrentHP : 0, Sanity = unit.CurrentSanity });
            }
            Memory.FirstFightComplete = true;
            CurrentStage = Stage.Debrief;
            _crate.SetActive(false);
            Status = reason;
            if (PersistenceEnabled)
            {
                _storageMessage = AllianceCheckpoint.Save(Memory, out string error)
                    ? "Checkpoint saved. Continue here or reload fight two after reopening." : error;
            }
        }

        bool AssistedRescue(UnitController unit) => Near(unit, _guardPoint, 4) &&
            unit.ActionHistory.Exists(a => a.Action == "Attack" && a.TargetId == _captor.UnitId);

        public bool LoadCheckpoint(string path = null)
        {
            if (CurrentStage != Stage.Briefing) return false;
            if (!AllianceCheckpoint.Load(out var loaded, out string error, path)) { _storageMessage = error; return false; }
            Memory = loaded;
            if (error.Length > 0) _storageMessage = error;
            ClearActors();
            SpawnAllies(Memory, false);
            ConnectContexts();
            _crate.SetActive(false);
            CurrentStage = Stage.Debrief;
            Status = "Loaded the first fight's survivors and memories. No campaign data was touched.";
            return true;
        }

        public void BeginSecondFight()
        {
            if (CurrentStage != Stage.Debrief) return;
            ClearActors();
            SpawnAllies(Memory);
            _boss = Spawn("The Hollow", 2, new Vector3(5, 1.3f, 1), 510, 12, _violet, PrimitiveType.Cube);
            _boss.transform.localScale = new Vector3(1.4f, 2.1f, .7f);
            _boss.AttackRange = 2.5f;
            // The Hollow loses no sanity to ordinary hits, so its 510 HP decides how long it lasts.
            _boss.TalentImmuneEldritchHit = true;
            Spawn("Husk", 2, new Vector3(2, .8f, -2), 90, 8, _violet, PrimitiveType.Sphere);
            ConnectContexts();
            CurrentStage = Stage.SecondFight;
            CurrentOrder = Order.Advance;
            Charges = Memory.HasSalvage ? 2 : 0;
            Elapsed = 0;
            _nextPulse = 5;
            _pulseSequence = 0;
            _guardTurn = 0;
            _pulseTarget = null;
            _crate.SetActive(false);
            Status = "Kill the Hollow. Trusting guards intercept its pulse; wary guards stay back.";
        }

        void UpdateSecondFight()
        {
            if (!_boss.IsAlive && !_enemies.Exists(u => u.IsAlive))
            {
                // The victory line names anyone who did not make it instead of claiming everyone survived.
                var lost = _friends.FindAll(u => !u.IsAlive).ConvertAll(u => u.UnitName);
                Finish(true, lost.Count == 0
                    ? "The Hollow fell and everyone came through. Look at what your first decision changed."
                    : "The Hollow fell, but " + string.Join(" and ", lost) + " did not make it. Look at what your first decision changed.");
                return;
            }
            if (Elapsed >= 65) { Finish(false, "The breach consumed the crossing. You ran out of time."); return; }
            // A pulse dies with the Hollow that cast it.
            if (_pulseTarget != null && !_boss.IsAlive)
            {
                _pulseTarget = null;
                Status = "The Hollow fell before its pulse landed.";
            }
            if (_boss.IsAlive && Elapsed >= _nextPulse)
            {
                _nextPulse += 9;
                _pulseAt = Elapsed + 3;
                _pulseTarget = _commander;
                // Every second pulse threatens a guard, taking turns so each living guard gets marked.
                if (_pulseSequence++ % 2 == 1)
                {
                    var guards = _friends.FindAll(u => IsGuard(u) && u.IsAlive);
                    if (guards.Count > 0) _pulseTarget = guards[_guardTurn++ % guards.Count];
                }
                Status = "THE HOLLOW IS LOOKING AT " + _pulseTarget.UnitName.ToUpperInvariant() + ". Three seconds.";
            }
            if (PulsePending && Elapsed >= _pulseAt)
            {
                var victim = _pulseTarget;
                UnitController protector = null;
                if (victim == _commander)
                {
                    foreach (var unit in _friends)
                        if (IsGuard(unit) && unit.IsAlive && Memory.WillCover(unit.UnitName) && Near(unit, victim.transform.position, 2.4f))
                        { protector = unit; break; }
                }
                else if (CurrentOrder == Order.CoverGuards && Near(_commander, victim.transform.position, 2.4f))
                    protector = _commander;

                var receiver = protector != null ? protector : victim;
                int statedSanity = protector != null ? 6 : 20;
                int hpBefore = receiver.CurrentHP, sanityBefore = receiver.CurrentSanity;
                receiver.TakeDamage(protector != null ? 24 : 48, _boss);
                // TakeDamage already applies an ordinary hit's sanity loss; add only the rest of the stated cost.
                int sanityLeft = statedSanity - (sanityBefore - receiver.CurrentSanity);
                if (receiver.IsAlive && sanityLeft > 0) receiver.ModifySanity(-sanityLeft, "HollowPulse");
                PulseCount++;
                if (protector != null)
                {
                    CoverCount++;
                    if (IsGuard(victim) && victim.IsAlive)
                        Memory.Record(2, (int)Elapsed, victim.UnitName, AllianceFact.Covered);
                }
                // Report what actually landed: a virtue, the sanity floor or a death can change the base cost.
                string who = protector != null ? protector.UnitName + " took the pulse for " + victim.UnitName : victim.UnitName + " took the pulse alone";
                if (!receiver.IsAlive) Status = who + " and fell.";
                else
                {
                    int sanityLost = sanityBefore - receiver.CurrentSanity;
                    Status = who + ": lost " + (hpBefore - receiver.CurrentHP) + " HP and " + sanityLost + " sanity" +
                        (sanityLost < statedSanity && receiver.ActiveVirtue == VirtueType.Stalwart ? " (Stalwart blocked the rest)." : ".");
                }
                _pulseTarget = null;
                ObserveCasualties();
                if (!_commander.IsAlive) Finish(false, "Your commander fell to the pulse. The fight is lost.");
            }
        }

        void Finish(bool victory, string message)
        {
            CurrentStage = victory ? Stage.Complete : Stage.Defeat;
            Status = message;
            _pulseTarget = null;
            Paused = false;
        }

        void ObserveCasualties()
        {
            foreach (var unit in _friends)
                if (!unit.IsAlive && _fallen.Add(unit.UnitId))
                    Memory.Record(CurrentStage == Stage.FirstFight ? 1 : 2, (int)Elapsed, unit.UnitName, AllianceFact.Fell);
        }

        // Same witness rules as BattleManager: a death costs its own side sanity,
        // and anyone going Lost costs every survivor on both sides. Only during a fight,
        // so casualties rebuilt from a checkpoint are not mourned twice.
        void OnUnitDefeated(UnitDefeatedEvent defeated)
        {
            if (CurrentStage != Stage.FirstFight && CurrentStage != Stage.SecondFight) return;
            foreach (var unit in defeated.TeamId == 1 ? _friends : _enemies)
                if (unit != null && unit.IsAlive && unit.UnitId != defeated.UnitId) unit.OnWitnessAllyDeath(null);
        }

        void OnUnitLost(UnitLostEvent lost)
        {
            if (CurrentStage != Stage.FirstFight && CurrentStage != Stage.SecondFight) return;
            foreach (var unit in _all)
                if (unit != null && unit.IsAlive && unit.UnitId != lost.UnitId) unit.OnWitnessUnitLost();
        }

        public void Restart()
        {
            ClearActors();
            Memory = new AllianceMemory();
            CurrentStage = Stage.Briefing;
            CurrentOrder = Order.Wait;
            Elapsed = 0; Charges = 0; CoverCount = 0; PulseCount = 0;
            _pulseTarget = null; Paused = false; _collection = 0;
            _crate.SetActive(true);
            Status = "A new test run. The previous on-disk checkpoint is kept until another first fight finishes.";
            SpawnFirstFight();
        }

        void SpawnFirstFight()
        {
            SpawnAllies(null);
            _captor = Spawn("Captor", 2, new Vector3(.5f, .8f, 3), 235, 9, _violet, PrimitiveType.Cube);
            ConnectContexts();
        }

        void SpawnAllies(AllianceMemory checkpoint, bool patchUp = true)
        {
            _commander = Spawn("Commander", 1, new Vector3(-6, .8f, -1), AllianceMemory.MaxHealth("Commander"), 18, Color.white, PrimitiveType.Capsule);
            // The commander holds its nerve: KS's lowest class Comprehension, so a hit costs 2 sanity instead of 5.
            _commander.Data.Comprehension = 0.5f;
            _scout = Spawn("Ember scout", 1, new Vector3(-6, .8f, -3), AllianceMemory.MaxHealth("Ember scout"), 13, _amber, PrimitiveType.Capsule);
            _scout.AttackRange = 5;
            Spawn("Ash guard 1", 1, _guardPoint, AllianceMemory.MaxHealth("Ash guard 1"), 4, _blue, PrimitiveType.Cube);
            Spawn("Ash guard 2", 1, _guardPoint + new Vector3(0, 0, 1.7f), AllianceMemory.MaxHealth("Ash guard 2"), 4, _blue, PrimitiveType.Cube);
            if (checkpoint == null) return;
            foreach (var unit in _friends)
            {
                var saved = checkpoint.Survivors.Find(s => s.Id == unit.UnitName);
                if (saved.Health <= 0)
                {
                    unit.TakeDamage(unit.MaxHP + 100, null);
                    _fallen.Add(unit.UnitId);
                }
                else
                {
                    // A small, explicit between-fight patch-up; never resurrect a casualty.
                    unit.CurrentHP = Mathf.Min(unit.MaxHP, saved.Health + (patchUp ? 20 : 0));
                    unit.CurrentSanity = Mathf.Min(100, saved.Sanity + (patchUp ? 10 : 0));
                }
            }
        }

        UnitController Spawn(string id, int team, Vector3 position, int health, int damage, Color color, PrimitiveType shape)
        {
            var go = Shape(id, shape, position, new Vector3(.85f, .85f, .85f), color, _actors, _actorOwned);
            var data = ScriptableObject.CreateInstance<UnitData>();
            _actorOwned.Add(data);
            data.name = id; data.UnitName = id; data.UnitType = "prototype";
            data.MaxHP = health; data.AttackDamage = damage; data.AttackCooldown = 1.2f;
            data.MoveSpeed = 2.8f; data.AttackRange = 1.6f; data.BaseSanity = 100;
            var unit = go.AddComponent<UnitController>();
            var strike = go.AddComponent<LineRenderer>();
            strike.sharedMaterial = go.GetComponent<Renderer>().sharedMaterial;
            strike.startWidth = .065f; strike.endWidth = .025f;
            strike.positionCount = 2; strike.useWorldSpace = true; strike.enabled = false;
            unit.Initialise(data, team, _nextId++);
            unit.SetBehaviourTree(new EncounterAction(this));
            _all.Add(unit);
            (team == 1 ? _friends : _enemies).Add(unit);
            return unit;
        }

        void ConnectContexts()
        {
            foreach (var unit in _all)
                unit.SetContext(new BattleContext { Owner = unit, Grid = _grid,
                    Allies = unit.TeamId == 1 ? _friends : _enemies,
                    Enemies = unit.TeamId == 1 ? _enemies : _friends });
        }

        sealed class EncounterAction : BTNode
        {
            readonly AlliancePrototype _owner;
            public EncounterAction(AlliancePrototype owner) : base("Alliance encounter") { _owner = owner; }
            public override NodeState Tick(BattleContext context) { _owner.Act(context); return NodeState.Running; }
        }

        void Act(BattleContext context)
        {
            var unit = context.Owner;
            if (unit.TeamId == 1 && !IsGuard(unit))
            {
                if (CurrentStage == Stage.FirstFight)
                {
                    if (CurrentOrder == Order.Wait) return;
                    if (CurrentOrder == Order.Salvage) { Move(unit, _salvagePoint, .6f); return; }
                }
                if (CurrentStage == Stage.SecondFight && CurrentOrder == Order.CoverGuards)
                {
                    // Go to the guard the Hollow has marked; with no mark, stand by a guard who does not trust you yet.
                    UnitController guard = PulsePending && IsGuard(_pulseTarget) ? _pulseTarget : null;
                    if (guard == null) guard = _friends.Find(u => IsGuard(u) && u.IsAlive && !Memory.WillCover(u.UnitName));
                    if (guard == null) guard = _friends.Find(u => IsGuard(u) && u.IsAlive);
                    if (guard != null && !Near(unit, guard.transform.position, 1.6f)) { Move(unit, guard.transform.position, 1.2f); return; }
                }
            }
            if (IsGuard(unit) && CurrentStage == Stage.SecondFight)
            {
                if (Memory.WillCover(unit.UnitName))
                {
                    if (PulsePending && _pulseTarget == _commander && !Near(unit, _commander.transform.position, 1.8f))
                    { Move(unit, _commander.transform.position, 1.2f); return; }
                }
                else if (!Near(unit, unit.SpawnPosition, 1.5f)) { Move(unit, unit.SpawnPosition, .5f); return; }
            }

            UnitController target = null;
            float closest = float.MaxValue;
            foreach (var other in context.Enemies)
            {
                if (!other.IsTargetable) continue;
                float distance = Vector3.Distance(unit.transform.position, other.transform.position);
                if (distance < closest) { closest = distance; target = other; }
            }
            if (target == null) return;
            bool hold = IsGuard(unit) && (CurrentStage == Stage.FirstFight || !Memory.WillCover(unit.UnitName)) ||
                unit.TeamId == 1 && CurrentStage == Stage.SecondFight && CurrentOrder == Order.CoverGuards;
            if (closest <= unit.AttackRange) unit.PerformAttack(target);
            else if (!hold) Move(unit, target.transform.position, unit.AttackRange * .9f);
        }

        static bool IsGuard(UnitController unit) => unit.UnitName.StartsWith("Ash guard", System.StringComparison.Ordinal);
        static bool Near(UnitController unit, Vector3 point, float range) => unit != null && unit.IsAlive &&
            Vector3.Distance(unit.transform.position, point) <= range;
        static void Move(UnitController unit, Vector3 destination, float stop)
        {
            destination.y = unit.transform.position.y;
            if (Vector3.Distance(unit.transform.position, destination) <= stop) return;
            unit.transform.position = Vector3.MoveTowards(unit.transform.position, destination, unit.MoveSpeed * Time.deltaTime);
        }

        // What the unit's own mind is doing, shown next to its orders so the panel never claims more than the unit will do.
        static string MindTag(UnitController unit)
        {
            if (unit.ActiveAffliction != AfflictionType.None) return unit.ActiveAffliction.ToString();
            if (unit.SanityState == SanityState.Broken || unit.SanityState == SanityState.Afflicted) return unit.SanityState.ToString();
            return "";
        }

        void ClearActors()
        {
            foreach (var unit in _all) { if (unit == null) continue; unit.gameObject.SetActive(false); Destroy(unit.gameObject); }
            // Each fighter's material and unit data go with it, so retries do not pile them up.
            foreach (var item in _actorOwned) if (item != null) Destroy(item);
            _all.Clear(); _friends.Clear(); _enemies.Clear(); _fallen.Clear(); _actorOwned.Clear();
        }

        void BuildArena()
        {
            _actors = new GameObject("Prototype fighters").transform;
            _actors.SetParent(transform);
            _grid = gameObject.AddComponent<BattleGrid>();
            var cameraObject = new GameObject("Prototype camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(transform);
            ViewCamera = cameraObject.GetComponent<Camera>();
            ViewCamera.transform.position = new Vector3(0, 20, -15);
            ViewCamera.transform.LookAt(new Vector3(0, 0, .5f));
            ViewCamera.orthographic = true; ViewCamera.orthographicSize = 10.8f;
            ViewCamera.clearFlags = CameraClearFlags.SolidColor; ViewCamera.backgroundColor = new Color(.035f, .045f, .07f);
            Shape("Crossing", PrimitiveType.Cube, new Vector3(0, -.25f, 0), new Vector3(18, .5f, 12), new Color(.12f, .16f, .19f), transform);
            for (int i = 0; i < 7; i++)
                Shape("Broken stone", PrimitiveType.Cube, new Vector3(-8 + i * 2.7f, .25f, 5.6f),
                    new Vector3(1.3f, 1.3f + i % 3, .7f), new Color(.24f, .27f, .32f), transform);
            Shape("Ash refuge", PrimitiveType.Cylinder, _guardPoint + Vector3.down * .7f, new Vector3(3.7f, .07f, 3.7f), new Color(.12f, .35f, .4f), transform);
            _crate = Shape("Salvage", PrimitiveType.Cube, _salvagePoint, Vector3.one, _amber, transform);
            _pulseMarker = Shape("Pulse warning", PrimitiveType.Cylinder, Vector3.zero, new Vector3(3, .05f, 3), _violet, transform);
            _pulseMarker.SetActive(false);
        }

        GameObject Shape(string label, PrimitiveType shape, Vector3 position, Vector3 scale, Color tint, Transform parent, List<Object> owner = null)
        {
            var go = GameObject.CreatePrimitive(shape);
            go.name = label; go.transform.SetParent(parent); go.transform.position = position; go.transform.localScale = scale;
            var material = new Material(_unlitBase);
            material.color = tint; go.GetComponent<Renderer>().sharedMaterial = material; (owner ?? _owned).Add(material);
            Destroy(go.GetComponent<Collider>());
            return go;
        }

        void LateUpdate()
        {
            if (_pulseMarker == null) return;
            _pulseMarker.SetActive(PulsePending);
            if (PulsePending) _pulseMarker.transform.position = new Vector3(_pulseTarget.transform.position.x, .08f, _pulseTarget.transform.position.z);
            foreach (var unit in _all)
            {
                var strike = unit.GetComponent<LineRenderer>();
                strike.enabled = false;
                if (!unit.IsAlive || unit.ActionHistory.Count == 0) continue;
                var action = unit.ActionHistory[unit.ActionHistory.Count - 1];
                if (action.Action != "Attack" || Time.time - action.Timestamp > .2f) continue;
                var target = _all.Find(u => u.UnitId == action.TargetId);
                if (target == null) continue;
                strike.enabled = true;
                strike.SetPosition(0, unit.transform.position + Vector3.up * .3f);
                strike.SetPosition(1, target.transform.position + Vector3.up * .3f);
            }
        }

        void OnDestroy()
        {
            foreach (var item in _owned) if (item != null) Destroy(item);
            foreach (var item in _actorOwned) if (item != null) Destroy(item);
        }

        void OnGUI()
        {
            if (ViewCamera == null) return;
            EnsureStyles();
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 800f);
            var previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 800 * scale) / 2), Quaternion.identity, Vector3.one * scale);
            Panel(new Rect(20, 18, 1240, 106));
            GUI.Label(new Rect(40, 28, 900, 32), "KINDRED SIEGE  /  THE FIRST ALLIANCE", _title);
            GUI.Label(new Rect(40, 65, 1120, 46), Status, _body);
            GUI.Label(new Rect(1040, 30, 200, 25), CurrentStage + "  " + (int)Elapsed + "s", _small);

            Panel(new Rect(20, 140, 240, 390));
            GUI.Label(new Rect(36, 152, 205, 24), "SURVIVORS / HP + SANITY", _small);
            for (int i = 0; i < _friends.Count; i++)
            {
                var unit = _friends[i];
                float row = 185 + i * 83;
                GUI.Label(new Rect(36, row, 208, 24), Tag(unit) + "  " + unit.UnitName, _small);
                GUI.Label(new Rect(36, row + 23, 208, 20), unit.IsAlive
                    ? "HP " + unit.CurrentHP + "/" + unit.MaxHP + "   SAN " + unit.CurrentSanity : "FALLEN", _small);
                Bar(new Rect(36, row + 47, 205, 4), (float)unit.CurrentHP / unit.MaxHP, _blue);
                string behaviour = !unit.IsAlive ? "Will not return" : !IsGuard(unit) ? "Your orders" :
                    CurrentStage == Stage.FirstFight || CurrentStage == Stage.Briefing ? "Pinned at refuge" :
                    Memory.WillCover(unit.UnitName) ? "Will shield you" : "Keeping distance";
                string mind = unit.IsAlive ? MindTag(unit) : "";
                GUI.Label(new Rect(36, row + 53, 208, 21), mind.Length > 0 ? behaviour + " · " + mind : behaviour, _small);
            }

            // World labels use the same letterboxed coordinates as the HUD.
            _tagRects.Clear();
            foreach (var unit in _all)
            {
                if (!unit.IsAlive) continue;
                var p = ViewCamera.WorldToScreenPoint(unit.transform.position + Vector3.up * 1.3f);
                float x = (p.x - (Screen.width - 1280 * scale) / 2) / scale;
                float y = (Screen.height - p.y - (Screen.height - 800 * scale) / 2) / scale;
                var tag = new Rect(x - 30, y - 18, unit.TeamId == 2 ? 170 : 60, 30);
                bool overlap;
                do
                {
                    overlap = false;
                    foreach (var previousTag in _tagRects)
                        if (previousTag.Overlaps(tag)) { tag.y -= 31; overlap = true; break; }
                } while (overlap);
                _tagRects.Add(tag);
                // Enemies show their sanity too: witnessing deaths can still break them.
                GUI.Label(tag, unit.TeamId == 2 ? unit.UnitName + "  SAN " + unit.CurrentSanity : Tag(unit), _small);
                if (unit.TeamId == 2) Bar(new Rect(tag.x, tag.y + 23, 96, 4), (float)unit.CurrentHP / unit.MaxHP, _violet);
            }

            Panel(new Rect(20, 570, 1240, 210));
            if (CurrentStage == Stage.Briefing)
            {
                GUI.Label(new Rect(40, 588, 1160, 80), "Your Ember scout and the Ash guards were enemies yesterday. Today the same thing is hunting you both.\nFree the guards by killing their captor, or recover a disruptor before the breach closes. You have 22 seconds.\nNext fight: trust changes who covers whom. These names, models and rules are prototype placeholders.", _body);
                if (Button(40, 700, 270, "BEGIN FIGHT ONE")) BeginFirstFight();
                if (Button(330, 700, 270, "LOAD FIGHT-TWO CHECKPOINT")) LoadCheckpoint();
            }
            else if (CurrentStage == Stage.FirstFight || CurrentStage == Stage.SecondFight)
            {
                bool first = CurrentStage == Stage.FirstFight;
                GUI.Label(new Rect(40, 584, 1160, 48), first
                    ? "One squad, two routes. Rescue earns cover. Salvage gives two disruptor charges. Neither happens just by clicking."
                    : "Blue guards who remember a rescue move to shield you. Wary guards stay at their refuge.\nTake a marked guard's pulse yourself to earn their trust. A trusting guard is not invulnerable.", _body);
                GUI.enabled = !Paused;
                if (Button(40, 647, 300, first ? "1  RESCUE THE GUARDS" : "1  ADVANCE")) IssueOrder(first ? Order.Rescue : Order.Advance);
                if (Button(360, 647, 300, first ? "2  RECOVER SALVAGE" : "2  COVER THE GUARDS")) IssueOrder(first ? Order.Salvage : Order.CoverGuards);
                GUI.enabled = !Paused && !first && Charges > 0 && PulsePending;
                if (Button(680, 647, 300, "3  DISRUPT PULSE  (" + Charges + ")")) DisruptPulse();
                GUI.enabled = true;
                if (Button(1000, 647, 220, Paused ? "SPACE  RESUME" : "SPACE  PAUSE")) TogglePause();
                GUI.Label(new Rect(40, 715, 1160, 32), "ORDER: " + CurrentOrder + (first ? "   |   Extraction: " + _collection.ToString("0.0") + "/4s" : "   |   Pulses taken: " + PulseCount + "   |   Covers: " + CoverCount) +
                    (Paused ? "   |   PAUSED" : ""), _small);
            }
            else
            {
                GUI.Label(new Rect(40, 584, 1130, 26), "WHAT THEY REMEMBER  /  actual events, not a replay", _body);
                _historyScroll = GUI.BeginScrollView(new Rect(40, 615, 1160, 78), _historyScroll, new Rect(0, 0, 1120, Memory.Events.Count * 30));
                for (int i = 0; i < Memory.Events.Count; i++) GUI.Label(new Rect(0, i * 30, 1100, 28), AllianceMemory.Describe(Memory.Events[i]), _small);
                GUI.EndScrollView();
                if (CurrentStage == Stage.Debrief)
                {
                    if (Button(40, 706, 300, "BEGIN FIGHT TWO")) BeginSecondFight();
                }
                if (Button(360, 706, 300, "TRY A DIFFERENT CHOICE")) Restart();
                GUI.Label(new Rect(685, 703, 530, 58), CurrentStage == Stage.Debrief ? "+20 HP / +10 sanity for survivors. Fallen units stay fallen.\n" + _storageMessage : "No automatic victory. Kill every enemy before 65s; keep your commander alive.", _small);
            }
            if (CurrentStage == Stage.Briefing) GUI.Label(new Rect(625, 705, 590, 55), _storageMessage, _small);
            GUI.matrix = previous;
        }

        void EnsureStyles()
        {
            if (_title != null) return;
            _title = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, normal = { textColor = _amber } };
            _body = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true, normal = { textColor = new Color(.9f, .94f, .96f) } };
            _small = new GUIStyle(_body) { fontSize = 14 };
            _button = new GUIStyle(GUI.skin.button) { fontSize = 15, fontStyle = FontStyle.Bold };
        }
        static string Tag(UnitController unit)
        {
            switch (unit.UnitName)
            {
                case "Commander": return "C";
                case "Ember scout": return "E";
                case "Ash guard 1": return "A1";
                case "Ash guard 2": return "A2";
                default: return unit.UnitName;
            }
        }
        bool Button(float x, float y, float w, string label) => GUI.Button(new Rect(x, y, w, 43), label, _button);
        static void Panel(Rect rect) { GUI.color = new Color(.025f, .04f, .06f, .97f); GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = Color.white; }
        static void Bar(Rect rect, float amount, Color color)
        {
            GUI.color = new Color(.02f, .03f, .04f); GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = color; rect.width *= Mathf.Clamp01(amount); GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = Color.white;
        }
    }
}
