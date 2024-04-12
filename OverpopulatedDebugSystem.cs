using System.Runtime.CompilerServices;
using Colossal;
using Game.Buildings;
using Game.Common;
using Game.Prefabs;
using Game.Tools;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Game.Debug;
using HarmonyLib;
using Game.Citizens;

namespace Overpopulated;

[HarmonyPatch]
public partial class OverpopulatedDebugSystem : BaseDebugSystem
{
    private struct GarbageGizmoJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle m_EntityType;

        [ReadOnly]
        public ComponentTypeHandle<PrefabRef> m_PrefabType;

        [ReadOnly]
        public ComponentTypeHandle<ResidentialProperty> m_ResidentialPropertyType;

        [ReadOnly]
        public ComponentLookup<BuildingPropertyData> m_BuildingPropertyDatas;

        [ReadOnly]
        public ComponentTypeHandle<Game.Objects.Transform> m_TransformType;

        [ReadOnly]
        public BufferLookup<Renter> m_Renters;

        [ReadOnly]
        public ComponentLookup<Household> m_Households; // We need to check if renter is actually a household because Mixed Residentials contain also companies

        [ReadOnly]
        public GarbageParameterData m_GarbageParameterData;

        public bool m_AccumulatedOption;

        public bool m_ProduceOption;

        public GizmoBatcher m_GizmoBatcher;

        private void DrawGarbage(Game.Objects.Transform t, int value)
        {
            float3 position = t.m_Position;
            float num = (float)value / 2f;
            position.y += num / 2f;
            int num2 = m_GarbageParameterData.m_HappinessEffectBaseline + m_GarbageParameterData.m_HappinessEffectStep;
            UnityEngine.Color color = UnityEngine.Color.green;
            if (value > num2)
            {
                color = UnityEngine.Color.Lerp(UnityEngine.Color.green, UnityEngine.Color.red, math.saturate((float)(value - num2) * 1f / (float)m_GarbageParameterData.m_HappinessEffectStep * 9f));
            }
            m_GizmoBatcher.DrawWireCube(position, new float3(5f, num, 5f), color);
        }

        private void DrawConsume(Game.Objects.Transform t, float value)
        {
            float3 position = t.m_Position;
            float num = value / 3f;
            position.y += num / 2f;
            UnityEngine.Color color = UnityEngine.Color.Lerp(UnityEngine.Color.green, UnityEngine.Color.red, math.saturate(value / 20000f));
            m_GizmoBatcher.DrawWireCube(position, new float3(5f, num, 5f), color);
        }

        private void DrawOverpopulation(Game.Objects.Transform t, int value)
        {
            float3 position = t.m_Position;
            float num = (float)value * 10f;
            position.y += num / 2f;
            UnityEngine.Color color = UnityEngine.Color.red; //  UnityEngine.Color.Lerp(UnityEngine.Color.green, UnityEngine.Color.red, math.saturate(value / 20000f));
            m_GizmoBatcher.DrawWireCube(position, new float3(5f, num, 5f), color);
        }

        public int CalculateOverpopulation(Entity entity, Entity prefab)
        {
            // Get number of residential properties
            if (!m_BuildingPropertyDatas.HasComponent(prefab))
            {
                return 0;
            }
            BuildingPropertyData buildingPropertyData = m_BuildingPropertyDatas[prefab];
            int resProperties = buildingPropertyData.m_ResidentialProperties; // Warning! Simple version - no checking for zone type and scaling, etc.

            // Access Renters and calculate number of households
            if (!m_Renters.HasBuffer(entity))
            {
                return 0;
            }
            DynamicBuffer<Renter> dynamicBuffer = m_Renters[entity];
            // Occupied properties - need to iterate do filter out companies
            int numHouseholds = 0;
            for (int n = 0; n < dynamicBuffer.Length; n++)
            {
                if (m_Households.HasComponent(dynamicBuffer[n].m_Renter))
                {
                    numHouseholds++;
                }
            }
            /*
            if (numHouseholds > resProperties)
            {
                Mod.log.Info($"Prefab {prefab.Index}: {numHouseholds} vs {resProperties}");
            }
            */
            return math.max(0, numHouseholds - resProperties);
        }

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> nativeArray = chunk.GetNativeArray(m_EntityType);
            NativeArray<PrefabRef> nativeArrayPrefabRef = chunk.GetNativeArray(ref m_PrefabType);
            NativeArray<ResidentialProperty> nativeArrayResidentialProperty = chunk.GetNativeArray(ref m_ResidentialPropertyType);
            NativeArray<Game.Objects.Transform> nativeArrayTransform = chunk.GetNativeArray(ref m_TransformType);
            for (int i = 0; i < nativeArray.Length; i++)
            {
                Entity entity = nativeArray[i];
                Entity prefab = nativeArrayPrefabRef[i].m_Prefab;
                int overpopulation = CalculateOverpopulation(entity, prefab);
                if (overpopulation > 0) DrawOverpopulation(nativeArrayTransform[i], overpopulation);
            }
        }

        void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            Execute(in chunk, unfilteredChunkIndex, useEnabledMask, in chunkEnabledMask);
        }
    }

    private struct TypeHandle
    {
        [ReadOnly]
        public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<ResidentialProperty> __Game_Buildings_ResidentialProperty_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<Game.Objects.Transform> __Game_Objects_Transform_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentLookup<BuildingPropertyData> __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;

        [ReadOnly]
        public BufferLookup<Renter> __Game_Buildings_Renter_RO_BufferLookup;

        [ReadOnly]
        public ComponentLookup<Household> __Game_Citizens_Household_RO_ComponentLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
            __Game_Buildings_ResidentialProperty_RO_ComponentTypeHandle = state.GetComponentTypeHandle<ResidentialProperty>(isReadOnly: true);
            __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PrefabRef>(isReadOnly: true);
            __Game_Objects_Transform_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Objects.Transform>(isReadOnly: true);
            __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup = state.GetComponentLookup<BuildingPropertyData>(isReadOnly: true);
            __Game_Buildings_Renter_RO_BufferLookup = state.GetBufferLookup<Renter>(isReadOnly: true);
            __Game_Citizens_Household_RO_ComponentLookup = state.GetComponentLookup<Household>(isReadOnly: true);
        }
    }

    private EntityQuery m_BuildingGroup;

    private GizmosSystem m_GizmosSystem;

    //private Option m_AccumulatedOption;

    //private Option m_ProduceOption;

    private TypeHandle __TypeHandle;

    private static bool isHooked = false;

    public static OverpopulatedDebugSystem m_OverpopulatedSystem;

    public static Game.Debug.GarbageDebugSystem m_GarbageSystem;

    /* why the fuck reverse patches are not working?!?!?
    [HarmonyPatch(typeof(Game.Debug.BaseDebugSystem), "AddOption")]
    [HarmonyReversePatch]
    public static void BaseDebugSystem_AddOption(string displayName, bool defaultEnabled)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException("It's a stub");
    }
    */

    protected override void OnCreate()
    {
        base.OnCreate();
        __TypeHandle.__AssignHandles(ref base.CheckedStateRef); // from OnCreateForCompiler
        m_GizmosSystem = base.World.GetOrCreateSystemManaged<GizmosSystem>();
        m_BuildingGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[3]
            {
                ComponentType.ReadOnly<ResidentialProperty>(),
                ComponentType.ReadOnly<Game.Objects.Transform>(),
                ComponentType.ReadOnly<PrefabRef>(),
            },
            None = new ComponentType[5]
            {
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Condemned>(),
                ComponentType.ReadOnly<Abandoned>(),
                ComponentType.ReadOnly<Destroyed>(),
                ComponentType.ReadOnly<Temp>(),
            }
        });
        base.Enabled = false;
        //m_AccumulatedOption = AddOption("Option 1", defaultEnabled: false);
        //m_ProduceOption = AddOption("Option 2", defaultEnabled: false);
        Mod.log.Info("OverpopulatedDebugSystem created.");
    }

    public static void LateHook()
    {
        m_OverpopulatedSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<OverpopulatedDebugSystem>();
        m_GarbageSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Debug.GarbageDebugSystem>();
        //BaseDebugSystem_AddOption("Overpopulated", true);
        isHooked = true;
    }

    [HarmonyPatch(typeof(Game.Debug.GarbageDebugSystem), "OnUpdate")]
    [HarmonyPrefix]
    public static bool GarbageDebugSystem_OnUpdate()
    {
        if (!isHooked)
            LateHook();
        OverpopulatedDebugSystem.m_OverpopulatedSystem.OnUpdate();
        return false;
    }

    protected override void OnUpdate()
    {
        //Mod.log.Info($"OverpopulatedSystem.OnUpdate");
        if (!m_BuildingGroup.IsEmptyIgnoreFilter)
        {
            __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Buildings_ResidentialProperty_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Objects_Transform_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Buildings_Renter_RO_BufferLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            GarbageGizmoJob garbageGizmoJob = default(GarbageGizmoJob);
            garbageGizmoJob.m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle;
            garbageGizmoJob.m_ResidentialPropertyType = __TypeHandle.__Game_Buildings_ResidentialProperty_RO_ComponentTypeHandle;
            garbageGizmoJob.m_PrefabType = __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
            garbageGizmoJob.m_TransformType = __TypeHandle.__Game_Objects_Transform_RO_ComponentTypeHandle;
            garbageGizmoJob.m_BuildingPropertyDatas = __TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
            garbageGizmoJob.m_Renters = __TypeHandle.__Game_Buildings_Renter_RO_BufferLookup;
            garbageGizmoJob.m_Households = __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup;
            //garbageGizmoJob.m_AccumulatedOption = m_AccumulatedOption.enabled;
            //garbageGizmoJob.m_ProduceOption = m_ProduceOption.enabled;
            garbageGizmoJob.m_GizmoBatcher = m_GizmosSystem.GetGizmosBatcher(out var dependencies);
            garbageGizmoJob.m_GarbageParameterData = GetEntityQuery(ComponentType.ReadOnly<GarbageParameterData>()).GetSingleton<GarbageParameterData>();
            GarbageGizmoJob jobData = garbageGizmoJob;
            base.Dependency = JobChunkExtensions.ScheduleParallel(jobData, m_BuildingGroup, JobHandle.CombineDependencies(base.Dependency, dependencies));
            m_GizmosSystem.AddGizmosBatcherWriter(base.Dependency);
        }
    }

    public OverpopulatedDebugSystem()
    {
    }
}