using System.Runtime.CompilerServices;
using Game.Buildings;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Prefabs;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Game;
using Game.Objects;

namespace Overpopulated;

public partial class OverpopulatedBuildingsSystem : GameSystemBase
{
    public struct OverpopulatedData
    {
        public int properties;
        public int households;
        public Entity prefabEntity;
        public int2 lotSize;
        public int level;
        public float zoneSpaceMultiplier;
        public float zoneResProperties;

        public OverpopulatedData()
        {
            prefabEntity = Entity.Null;
        }
    }

    private struct IdentifyOverpopulatedJob : IJob
    {
        [ReadOnly]
        public NativeList<ArchetypeChunk> m_ResidentialChunks;

        [ReadOnly]
        public BufferTypeHandle<Renter> m_RenterType;

        [ReadOnly]
        public ComponentTypeHandle<PrefabRef> m_PrefabType;

        [ReadOnly]
        public ComponentTypeHandle<PropertyRenter> m_PropertyRenterType;

        [ReadOnly]
        public ComponentLookup<BuildingPropertyData> m_BuildingPropertyDatas;

        [ReadOnly]
        public ComponentLookup<Household> m_Households;

        [ReadOnly]
        public ComponentLookup<Population> m_Populations;

        [ReadOnly]
        public ComponentLookup<SpawnableBuildingData> m_SpawnableDatas;

        [ReadOnly]
        public ComponentLookup<BuildingData> m_BuildingDatas;

        [ReadOnly]
        public ComponentLookup<Game.Objects.Transform> m_TransformDatas;

        [ReadOnly]
        public ComponentLookup<ZonePropertiesData> m_ZonePropertyDatas;

        public NativeArray<OverpopulatedData> m_Results;

        public void Execute()
        {
            int numBuildingsReported = 0, delta = 0;
            for (int l = 0; l < m_ResidentialChunks.Length; l++)
            {
                ArchetypeChunk archetypeChunk2 = m_ResidentialChunks[l];
                NativeArray<PrefabRef> nativeArray = archetypeChunk2.GetNativeArray(ref m_PrefabType);
                BufferAccessor<Renter> bufferAccessor = archetypeChunk2.GetBufferAccessor(ref m_RenterType);
                //NativeArray<Transform> nativeArrayTransforms = archetypeChunk2.GetNativeArray(ref m_TransformType);
                // iterate through buildings
                for (int m = 0; m < nativeArray.Length; m++)
                {
                    Entity prefabEntity = nativeArray[m].m_Prefab;
                    SpawnableBuildingData spawnableBuildingData = m_SpawnableDatas[prefabEntity];
                    ZonePropertiesData zonePropertiesData = m_ZonePropertyDatas[spawnableBuildingData.m_ZonePrefab];
                    float num10 = zonePropertiesData.m_ResidentialProperties / zonePropertiesData.m_SpaceMultiplier;
                    if (!m_BuildingPropertyDatas.HasComponent(prefabEntity))
                    {
                        continue;
                    }
                    BuildingPropertyData buildingPropertyData = m_BuildingPropertyDatas[prefabEntity];
                    DynamicBuffer<Renter> dynamicBuffer = bufferAccessor[m];
					// occupied properties
                    int numHouseholds = 0;
                    for (int n = 0; n < dynamicBuffer.Length; n++)
                    {
                        if (m_Households.HasComponent(dynamicBuffer[n].m_Renter))
                        {
                            numHouseholds++;
                        }
                    }
                    int resProperties = 0;
                    if (!zonePropertiesData.m_ScaleResidentials)
                    {
                        // low density (not scalable, only 1 household per buildinng)
                        resProperties = 1;
                    }
                    else if (num10 < 1f)
                    {
                        // medium density, scaling < 1f
                        resProperties = buildingPropertyData.m_ResidentialProperties;
                    }
                    else
                    {
                        // high density, scaling >= 1f
                        resProperties = buildingPropertyData.m_ResidentialProperties;
                    }

                    //Transform transform = m_Transforms[entity];
                    BuildingData buildingData = m_BuildingDatas[prefabEntity];
                    int lotSize = buildingData.m_LotSize.x * buildingData.m_LotSize.y;
                    if (numHouseholds > resProperties)
                    {
                        //Mod.log.Info($"Prefab {prefabEntity.Index}: {numHouseholds} vs {buildingPropertyData.m_ResidentialProperties} " +
                            //$"lot {buildingData.m_LotSize.x}x{buildingData.m_LotSize.y} bspc {buildingPropertyData.m_SpaceMultiplier} lv {spawnableBuildingData.m_Level} zspc {zonePropertiesData.m_SpaceMultiplier} zres {zonePropertiesData.m_ResidentialProperties}");
                        if (numBuildingsReported < m_Results.Length)
                        {
                            OverpopulatedData data = new OverpopulatedData();
                            data.properties = resProperties;
                            data.households = numHouseholds;
                            data.prefabEntity = prefabEntity;
                            data.lotSize = buildingData.m_LotSize;
                            data.level = spawnableBuildingData.m_Level;
                            data.zoneSpaceMultiplier = zonePropertiesData.m_SpaceMultiplier;
                            data.zoneResProperties = zonePropertiesData.m_ResidentialProperties;
                            m_Results[numBuildingsReported] = data;
                        }
                        numBuildingsReported++;
                        delta += numHouseholds - resProperties;
                    }
                } // buildings
            } // chunks
            //Mod.log.Info($"Total: {numBuildingsReported} buildings {delta} too many households");
        }
    }

    private struct TypeHandle
    {
        [ReadOnly]
        public BufferTypeHandle<Renter> __Game_Buildings_Renter_RO_BufferTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentLookup<BuildingPropertyData> __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Household> __Game_Citizens_Household_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Population> __Game_City_Population_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<SpawnableBuildingData> __Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<BuildingData> __Game_Prefabs_BuildingData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<ZonePropertiesData> __Game_Prefabs_ZonePropertiesData_RO_ComponentLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Game_Buildings_Renter_RO_BufferTypeHandle = state.GetBufferTypeHandle<Renter>(isReadOnly: true);
            __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PrefabRef>(isReadOnly: true);
            __Game_Buildings_PropertyRenter_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PropertyRenter>(isReadOnly: true);
            __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup = state.GetComponentLookup<BuildingPropertyData>(isReadOnly: true);
            __Game_Citizens_Household_RO_ComponentLookup = state.GetComponentLookup<Household>(isReadOnly: true);
            __Game_City_Population_RO_ComponentLookup = state.GetComponentLookup<Population>(isReadOnly: true);
            __Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup = state.GetComponentLookup<SpawnableBuildingData>(isReadOnly: true);
            __Game_Prefabs_BuildingData_RO_ComponentLookup = state.GetComponentLookup<BuildingData>(isReadOnly: true);
            __Game_Prefabs_ZonePropertiesData_RO_ComponentLookup = state.GetComponentLookup<ZonePropertiesData>(isReadOnly: true);
            //__Game_Objects_Transform_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Objects.Transform>(isReadOnly: true);
        }
    }

    private PrefabSystem m_PrefabSystem;

    private EntityQuery m_AllResidentialGroup;

    private TypeHandle __TypeHandle;

    private NativeArray<OverpopulatedData> m_Results;

    protected override void OnCreate()
    {
        base.OnCreate();
        __TypeHandle.__AssignHandles(ref base.CheckedStateRef);
        m_AllResidentialGroup = GetEntityQuery(ComponentType.ReadOnly<ResidentialProperty>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Condemned>(), ComponentType.Exclude<Abandoned>(), ComponentType.Exclude<Destroyed>(), ComponentType.Exclude<Temp>());
        m_PrefabSystem = base.World.GetOrCreateSystemManaged<PrefabSystem>();
        m_Results = new NativeArray<OverpopulatedData>(1000, Allocator.Persistent);
        base.Enabled = false;
        Mod.log.Info("OverpopulatedBuildingsSystem created.");
    }

    protected override void OnDestroy()
    {
        m_Results.Dispose();
        base.OnDestroy();
    }

    protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
    {
        base.OnGamePreload(purpose, mode);
        Mod.log.Info($"OnGamePreload: mode {mode} purpose {purpose}");
    }

    protected override void OnGameLoaded(Colossal.Serialization.Entities.Context serializationContext)
    {
        base.OnGameLoaded(serializationContext);
        Mod.log.Info($"OnGameLoaded: version {serializationContext.version} purpose {serializationContext.purpose}");
    }

    protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
    {
        base.OnGameLoadingComplete(purpose, mode);
        Mod.log.Info($"OnGameLoadingComplete: mode {mode} purpose {purpose}");
        if (mode == GameMode.Game && purpose == Colossal.Serialization.Entities.Purpose.LoadGame)
            base.Enabled = true;
    }

    protected override void OnUpdate()
    {
        Mod.log.Info($"OnUpdate: run once");
        for (int i = 0; i < m_Results.Length; i++)
            m_Results[i] = new OverpopulatedData();

        __TypeHandle.__Game_Prefabs_ZonePropertiesData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_City_Population_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_Renter_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
        IdentifyOverpopulatedJob identifyOverpopulatedJob = default(IdentifyOverpopulatedJob);
        identifyOverpopulatedJob.m_ResidentialChunks = m_AllResidentialGroup.ToArchetypeChunkListAsync(base.World.UpdateAllocator.ToAllocator, out var outJobHandle);
        identifyOverpopulatedJob.m_RenterType = __TypeHandle.__Game_Buildings_Renter_RO_BufferTypeHandle;
        identifyOverpopulatedJob.m_PrefabType = __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
        identifyOverpopulatedJob.m_PropertyRenterType = __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle;
        identifyOverpopulatedJob.m_BuildingPropertyDatas = __TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
        identifyOverpopulatedJob.m_Households = __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup;
        identifyOverpopulatedJob.m_Populations = __TypeHandle.__Game_City_Population_RO_ComponentLookup;
        identifyOverpopulatedJob.m_SpawnableDatas = __TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;
        identifyOverpopulatedJob.m_BuildingDatas = __TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup;
        identifyOverpopulatedJob.m_ZonePropertyDatas = __TypeHandle.__Game_Prefabs_ZonePropertiesData_RO_ComponentLookup;
        identifyOverpopulatedJob.m_Results = m_Results;
        IJobExtensions.Schedule(identifyOverpopulatedJob, outJobHandle).Complete();
        base.Enabled = false;

        Mod.log.Info($"FULL REPORT");
        int numBuildingsReported = 0, delta = 0;
        for (int i = 0; i < m_Results.Length; i++)
        {
            OverpopulatedData data = m_Results[i];
            if (data.households > 0)
            {
                string prefabName = data.prefabEntity.ToString();
                if (m_PrefabSystem.TryGetPrefab(data.prefabEntity, out PrefabBase prefab))
                    prefabName = prefab.name;
                Mod.log.Info($"{prefabName}: {data.households} vs {data.properties} lot {data.lotSize.x}x{data.lotSize.y} lv {data.level} zspc {data.zoneSpaceMultiplier} zres {data.zoneResProperties}");
                numBuildingsReported++;
                delta += data.households - data.properties;
            }
        }
        Mod.log.Info($"Total: {numBuildingsReported} buildings {delta} too many households");
    }

    public OverpopulatedBuildingsSystem()
    {
    }
}
