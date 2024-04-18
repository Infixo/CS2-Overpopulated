using System.Runtime.CompilerServices;
using Game.Buildings;
using Game.City;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Scripting;
using Game;
using Game.Serialization;
using Game.Prefabs;
using UnityEngine;
using Game.Agents;

namespace Overpopulated;

//[CompilerGenerated]
public partial class RenterSystem : GameSystemBase
{
    //[BurstCompile]
    private struct RenterJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle m_EntityType;

        [ReadOnly]
        public ComponentTypeHandle<PropertyRenter> m_PropertyRenterType;

        [ReadOnly]
        public ComponentLookup<CityServiceUpkeep> m_ServiceBuildings;

        [ReadOnly]
        public ComponentLookup<PrefabRef> m_PrefabRefs;

        [ReadOnly]
        public ComponentLookup<BuildingPropertyData> m_BuildingPropertyDatas;

        [ReadOnly]
        public ComponentLookup<ParkData> m_ParkDatas;

        [ReadOnly]
        public ComponentLookup<Game.Citizens.Household> m_Households;

        public BufferLookup<Renter> m_Renters;

        public EntityCommandBuffer m_CommandBuffer;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> nativeArray = chunk.GetNativeArray(m_EntityType);
            NativeArray<PropertyRenter> nativeArray2 = chunk.GetNativeArray(ref m_PropertyRenterType);
            for (int i = 0; i < nativeArray2.Length; i++)
            {
                Entity entity = nativeArray[i]; // household
                PropertyRenter propertyRenter = nativeArray2[i];
                if (m_Renters.HasBuffer(propertyRenter.m_Property))
                {
                    // 240418 The job recreates Renter buffer in properties based on info from all PropertyRenters.
                    // It doesn't check if there is a capcity in the property and this creates Overpopulated properties.
                    // The fix checks if there is free space in the building and only then adds a renter.
                    // If there is no free space, the renter is removed from the building and marked as PropertySeeker.
                    // Must also account for parks that can house homeless families.

                    if (!m_PrefabRefs.TryGetComponent(propertyRenter.m_Property, out PrefabRef prefabRef))
                    {
                        Mod.log.Warn($"Failed to retrieve PrefabRef from {entity}.");
                        continue;
                    }
                    int numProperties = 0;
                    if (m_BuildingPropertyDatas.TryGetComponent(prefabRef.m_Prefab, out BuildingPropertyData buildingPropertyData))
                    {
                        numProperties = buildingPropertyData.CountProperties();
                    }
                    else if (m_ParkDatas.TryGetComponent(prefabRef.m_Prefab, out ParkData parkData))
                    {
                        // If homeless are allowed, then there is no limit.
                        numProperties = parkData.m_AllowHomeless ? int.MaxValue : 0;
                    }
                    else
                    {
                        Mod.log.Warn($"Failed to retrieve BuildingPropertyData and ParkData from {prefabRef.m_Prefab}.");
                        continue;
                    }

                    DynamicBuffer<Renter> renterBuffer = m_Renters[propertyRenter.m_Property];

                    if (renterBuffer.Length < numProperties)
                    {
                        m_Renters[propertyRenter.m_Property].Add(new Renter
                        {
                            m_Renter = entity
                        });
#if DEBUG
                        Mod.log.Info($"Adding {entity} to property {propertyRenter.m_Property} ({numProperties}) -> {renterBuffer.Length}");
#endif
                    }
                    else
                    {
                        m_CommandBuffer.RemoveComponent<PropertyRenter>(entity);
                        m_CommandBuffer.AddComponent(entity, default(PropertySeeker));
                        Mod.log.Info($"Removing {entity} from {propertyRenter.m_Property} (has {renterBuffer.Length}, max is {numProperties}).");
                    }
                }
                else if (!m_ServiceBuildings.HasComponent(entity))
                {
                    m_CommandBuffer.RemoveComponent<PropertyRenter>(entity);
                }
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
        public ComponentTypeHandle<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentTypeHandle;

        public BufferLookup<Renter> __Game_Buildings_Renter_RW_BufferLookup;

        [ReadOnly]
        public ComponentLookup<CityServiceUpkeep> __Game_City_CityServiceUpkeep_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Game.Prefabs.PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Game.Prefabs.BuildingPropertyData> __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Game.Prefabs.ParkData> __Game_Prefabs_ParkData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Game.Citizens.Household> __Game_Citizens_Household_RO_ComponentLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
            __Game_Buildings_PropertyRenter_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PropertyRenter>(isReadOnly: true);
            __Game_Buildings_Renter_RW_BufferLookup = state.GetBufferLookup<Renter>();
            __Game_City_CityServiceUpkeep_RO_ComponentLookup = state.GetComponentLookup<CityServiceUpkeep>(isReadOnly: true);
            __Game_Prefabs_PrefabRef_RO_ComponentLookup = state.GetComponentLookup<PrefabRef>(isReadOnly: true);
            __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup = state.GetComponentLookup<BuildingPropertyData>(isReadOnly: true);
            __Game_Prefabs_ParkData_RO_ComponentLookup = state.GetComponentLookup<ParkData>(isReadOnly: true);
            __Game_Citizens_Household_RO_ComponentLookup = state.GetComponentLookup<Game.Citizens.Household>(isReadOnly: true);
        }
    }

    private DeserializationBarrier m_DeserializationBarrier;

    private EntityQuery m_Query;

    private TypeHandle __TypeHandle;

    //[Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();
        m_DeserializationBarrier = base.World.GetOrCreateSystemManaged<DeserializationBarrier>();
        m_Query = GetEntityQuery(ComponentType.ReadOnly<PropertyRenter>());
        RequireForUpdate(m_Query);
        Mod.log.Info("Modded RenterSystem created.");
    }

    //[Preserve]
    protected override void OnUpdate()
    {
        __TypeHandle.__Game_City_CityServiceUpkeep_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_Renter_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        RenterJob renterJob = default(RenterJob);
        renterJob.m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle;
        renterJob.m_PropertyRenterType = __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle;
        renterJob.m_Renters = __TypeHandle.__Game_Buildings_Renter_RW_BufferLookup;
        renterJob.m_ServiceBuildings = __TypeHandle.__Game_City_CityServiceUpkeep_RO_ComponentLookup;
        renterJob.m_PrefabRefs = __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup;
        renterJob.m_BuildingPropertyDatas = __TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
        renterJob.m_ParkDatas = __TypeHandle.__Game_Prefabs_ParkData_RO_ComponentLookup;
        renterJob.m_Households = __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup;
        renterJob.m_CommandBuffer = m_DeserializationBarrier.CreateCommandBuffer();
        RenterJob jobData = renterJob;
        base.Dependency = JobChunkExtensions.Schedule(jobData, m_Query, base.Dependency);
        m_DeserializationBarrier.AddJobHandleForProducer(base.Dependency);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void __AssignQueries(ref SystemState state)
    {
    }

    protected override void OnCreateForCompiler()
    {
        base.OnCreateForCompiler();
        __AssignQueries(ref base.CheckedStateRef);
        __TypeHandle.__AssignHandles(ref base.CheckedStateRef);
    }

    //[Preserve]
    public RenterSystem()
    {
    }
}
