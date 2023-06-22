using KSerialization;
using System;
using UnityEngine;

namespace SuperElectrolyzer.Buildings
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class SuperElectrolyzer : StateMachineComponent<SuperElectrolyzer.StatesInstance>
    {
        protected override void OnSpawn()
        {
            KBatchedAnimController component = base.GetComponent<KBatchedAnimController>();
            if (this.hasMeter)
            {
                this.meter = new MeterController(component, "U2H_meter_target", "meter", Meter.Offset.Behind, Grid.SceneLayer.NoLayer, new Vector3(-0.4f, 0.5f, -0.1f), new string[]
                {
                "U2H_meter_target",
                "U2H_meter_tank",
                "U2H_meter_waterbody",
                "U2H_meter_level"
                });
            }
            base.smi.StartSM();
            this.UpdateMeter();
            Tutorial.Instance.oxygenGenerators.Add(base.gameObject);
        }
        protected override void OnCleanUp()
        {
            Tutorial.Instance.oxygenGenerators.Remove(base.gameObject);
            base.OnCleanUp();
        }
        public void UpdateMeter()
        {
            if (this.hasMeter)
            {
                float positionPercent = Mathf.Clamp01(this.storage.MassStored() / this.storage.capacityKg);
                this.meter.SetPositionPercent(positionPercent);
            }
        }
        private bool RoomForPressure
        {
            get
            {
                int num = Grid.PosToCell(base.transform.GetPosition());
                num = Grid.OffsetCell(num, this.emissionOffset);
                return !GameUtil.FloodFillCheck<SuperElectrolyzer>(new Func<int, SuperElectrolyzer, bool>(SuperElectrolyzer.OverPressure), this, num, 3, true, true);
            }
        }
        private static bool OverPressure(int cell, SuperElectrolyzer electrolyzer)
        {
            return Grid.Mass[cell] > electrolyzer.maxMass;
        }
        [SerializeField]
        public float maxMass = 2.5f;
        [SerializeField]
        public bool hasMeter = true;
        [SerializeField]
        public CellOffset emissionOffset = CellOffset.none;
        [MyCmpAdd]
        private Storage storage;
        [MyCmpGet]
        private ElementConverter emitter;
        [MyCmpReq]
        private Operational operational;
        private MeterController meter;
        public class StatesInstance : GameStateMachine<SuperElectrolyzer.States, SuperElectrolyzer.StatesInstance, SuperElectrolyzer, object>.GameInstance
        {
            public StatesInstance(SuperElectrolyzer smi) : base(smi)
            {
            }
        }
        public class States : GameStateMachine<SuperElectrolyzer.States, SuperElectrolyzer.StatesInstance, SuperElectrolyzer>
        {
            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                default_state = this.disabled;
                this.root.EventTransition(GameHashes.OperationalChanged, this.disabled, (SuperElectrolyzer.StatesInstance smi) => !smi.master.operational.IsOperational).EventHandler(GameHashes.OnStorageChange, delegate (SuperElectrolyzer.StatesInstance smi)
                {
                    smi.master.UpdateMeter();
                });
                this.disabled.EventTransition(GameHashes.OperationalChanged, this.waiting, (SuperElectrolyzer.StatesInstance smi) => smi.master.operational.IsOperational);
                this.waiting.Enter("Waiting", delegate (SuperElectrolyzer.StatesInstance smi)
                {
                    smi.master.operational.SetActive(false, false);
                }).EventTransition(GameHashes.OnStorageChange, this.converting, (SuperElectrolyzer.StatesInstance smi) => smi.master.GetComponent<ElementConverter>().HasEnoughMassToStartConverting(false));
                this.converting.Enter("Ready", delegate (SuperElectrolyzer.StatesInstance smi)
                {
                    smi.master.operational.SetActive(true, false);
                }).Transition(this.waiting, (SuperElectrolyzer.StatesInstance smi) => !smi.master.GetComponent<ElementConverter>().CanConvertAtAll(), UpdateRate.SIM_200ms).Transition(this.overpressure, (SuperElectrolyzer.StatesInstance smi) => !smi.master.RoomForPressure, UpdateRate.SIM_200ms);
                this.overpressure.Enter("OverPressure", delegate (SuperElectrolyzer.StatesInstance smi)
                {
                    smi.master.operational.SetActive(false, false);
                }).ToggleStatusItem(Db.Get().BuildingStatusItems.PressureOk, null).Transition(this.converting, (SuperElectrolyzer.StatesInstance smi) => smi.master.RoomForPressure, UpdateRate.SIM_200ms);
            }
            public GameStateMachine<SuperElectrolyzer.States, SuperElectrolyzer.StatesInstance, SuperElectrolyzer, object>.State disabled;
            public GameStateMachine<SuperElectrolyzer.States, SuperElectrolyzer.StatesInstance, SuperElectrolyzer, object>.State waiting;
            public GameStateMachine<SuperElectrolyzer.States, SuperElectrolyzer.StatesInstance, SuperElectrolyzer, object>.State converting;
            public GameStateMachine<SuperElectrolyzer.States, SuperElectrolyzer.StatesInstance, SuperElectrolyzer, object>.State overpressure;
        }
    }

}
