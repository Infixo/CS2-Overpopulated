using System.Runtime.CompilerServices;
using Game.Buildings;
using Game.Common;
using Game.Tools;
using Game.Vehicles;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Scripting;
using Game;
using Game.Citizens;

namespace Overpopulated;

//[CompilerGenerated]
public partial class HouseholdRemoveSystem : GameSystemBase
{
    [BurstCompile]
    private struct RemoveHouseholdJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle m_EntityType;

        [ReadOnly]
        public ComponentTypeHandle<PropertyRenter> m_PropertyRenterType;

        [ReadOnly]
        public BufferTypeHandle<HouseholdCitizen> m_HouseholdCitizenType;

        [ReadOnly]
        public BufferTypeHandle<HouseholdAnimal> m_HouseholdAnimalType;

        [ReadOnly]
        public BufferTypeHandle<OwnedVehicle> m_OwnedVehicleType;

        [ReadOnly]
        public ComponentLookup<HouseholdPet> m_HouseholdPets;

        [ReadOnly]
        public ComponentLookup<Citizen> m_Citizens;

        [ReadOnly]
        public ComponentLookup<Vehicle> m_Vehicles;

        [ReadOnly]
        public BufferLookup<LayoutElement> m_LayoutElements;

        public BufferLookup<Renter> m_Renters;

        [ReadOnly]
        public EntityArchetype m_RentEventArchetype;

        public EntityCommandBuffer m_CommandBuffer;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> nativeArray = chunk.GetNativeArray(m_EntityType);
            NativeArray<PropertyRenter> nativeArray2 = chunk.GetNativeArray(ref m_PropertyRenterType);
            BufferAccessor<HouseholdCitizen> bufferAccessor = chunk.GetBufferAccessor(ref m_HouseholdCitizenType);
            BufferAccessor<HouseholdAnimal> bufferAccessor2 = chunk.GetBufferAccessor(ref m_HouseholdAnimalType);
            BufferAccessor<OwnedVehicle> bufferAccessor3 = chunk.GetBufferAccessor(ref m_OwnedVehicleType);
            for (int i = 0; i < nativeArray.Length; i++)
            {
                if (bufferAccessor2.Length > 0)
                {
                    DynamicBuffer<HouseholdAnimal> dynamicBuffer = bufferAccessor2[i];
                    for (int j = 0; j < dynamicBuffer.Length; j++)
                    {
                        if (m_HouseholdPets.HasComponent(dynamicBuffer[j].m_HouseholdPet))
                        {
                            m_CommandBuffer.AddComponent(dynamicBuffer[j].m_HouseholdPet, default(Deleted));
                        }
                    }
                }
                if (bufferAccessor.Length > 0)
                {
                    DynamicBuffer<HouseholdCitizen> dynamicBuffer2 = bufferAccessor[i];
                    for (int k = 0; k < dynamicBuffer2.Length; k++)
                    {
                        if (m_Citizens.HasComponent(dynamicBuffer2[k].m_Citizen))
                        {
                            m_CommandBuffer.AddComponent(dynamicBuffer2[k].m_Citizen, default(Deleted));
                        }
                    }
                }
                if (bufferAccessor3.Length > 0)
                {
                    DynamicBuffer<OwnedVehicle> dynamicBuffer3 = bufferAccessor3[i];
                    for (int l = 0; l < dynamicBuffer3.Length; l++)
                    {
                        Entity vehicle = dynamicBuffer3[l].m_Vehicle;
                        if (m_Vehicles.HasComponent(vehicle))
                        {
                            DynamicBuffer<LayoutElement> layout = default(DynamicBuffer<LayoutElement>);
                            if (m_LayoutElements.HasBuffer(vehicle))
                            {
                                layout = m_LayoutElements[vehicle];
                            }
                            VehicleUtils.DeleteVehicle(m_CommandBuffer, vehicle, layout);
                        }
                    }
                }
                if (nativeArray2.Length <= 0)
                {
                    continue;
                }
                Entity entity = nativeArray[i];
                PropertyRenter propertyRenter = nativeArray2[i];
                if (!m_Renters.TryGetBuffer(propertyRenter.m_Property, out var bufferData))
                {
                    continue;
                }
                for (int m = 0; m < bufferData.Length; m++)
                {
                    if (bufferData[m].m_Renter == entity)
                    {
                        bufferData.RemoveAt(m);
                        break;
                    }
                }
                Entity e = m_CommandBuffer.CreateEntity(m_RentEventArchetype);
                m_CommandBuffer.SetComponent(e, new RentersUpdated(propertyRenter.m_Property));
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

        [ReadOnly]
        public BufferTypeHandle<HouseholdCitizen> __Game_Citizens_HouseholdCitizen_RO_BufferTypeHandle;

        [ReadOnly]
        public BufferTypeHandle<HouseholdAnimal> __Game_Citizens_HouseholdAnimal_RO_BufferTypeHandle;

        [ReadOnly]
        public BufferTypeHandle<OwnedVehicle> __Game_Vehicles_OwnedVehicle_RO_BufferTypeHandle;

        [ReadOnly]
        public ComponentLookup<HouseholdPet> __Game_Citizens_HouseholdPet_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Citizen> __Game_Citizens_Citizen_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Vehicle> __Game_Vehicles_Vehicle_RO_ComponentLookup;

        [ReadOnly]
        public BufferLookup<LayoutElement> __Game_Vehicles_LayoutElement_RO_BufferLookup;

        public BufferLookup<Renter> __Game_Buildings_Renter_RW_BufferLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
            __Game_Buildings_PropertyRenter_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PropertyRenter>(isReadOnly: true);
            __Game_Citizens_HouseholdCitizen_RO_BufferTypeHandle = state.GetBufferTypeHandle<HouseholdCitizen>(isReadOnly: true);
            __Game_Citizens_HouseholdAnimal_RO_BufferTypeHandle = state.GetBufferTypeHandle<HouseholdAnimal>(isReadOnly: true);
            __Game_Vehicles_OwnedVehicle_RO_BufferTypeHandle = state.GetBufferTypeHandle<OwnedVehicle>(isReadOnly: true);
            __Game_Citizens_HouseholdPet_RO_ComponentLookup = state.GetComponentLookup<HouseholdPet>(isReadOnly: true);
            __Game_Citizens_Citizen_RO_ComponentLookup = state.GetComponentLookup<Citizen>(isReadOnly: true);
            __Game_Vehicles_Vehicle_RO_ComponentLookup = state.GetComponentLookup<Vehicle>(isReadOnly: true);
            __Game_Vehicles_LayoutElement_RO_BufferLookup = state.GetBufferLookup<LayoutElement>(isReadOnly: true);
            __Game_Buildings_Renter_RW_BufferLookup = state.GetBufferLookup<Renter>();
        }
    }

    private EntityQuery m_HouseholdQuery;

    private EntityArchetype m_RentEventArchetype;

    private ModificationBarrier2 m_ModificationBarrier;

    private TypeHandle __TypeHandle;

    //[Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();
        m_ModificationBarrier = base.World.GetOrCreateSystemManaged<ModificationBarrier2>();
        m_HouseholdQuery = GetEntityQuery(ComponentType.ReadOnly<Household>(), ComponentType.ReadOnly<Deleted>(), ComponentType.Exclude<Temp>());
        m_RentEventArchetype = base.EntityManager.CreateArchetype(ComponentType.ReadWrite<Event>(), ComponentType.ReadWrite<RentersUpdated>());
        RequireForUpdate(m_HouseholdQuery);
        Mod.log.Info("HouseholdRemoveSystem restored.");
    }

    //[Preserve]
    protected override void OnUpdate()
    {
        __TypeHandle.__Game_Buildings_Renter_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Vehicles_LayoutElement_RO_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Vehicles_Vehicle_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Citizen_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdPet_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Vehicles_OwnedVehicle_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdAnimal_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdCitizen_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
        RemoveHouseholdJob jobData = default(RemoveHouseholdJob);
        jobData.m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle;
        jobData.m_PropertyRenterType = __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle;
        jobData.m_HouseholdCitizenType = __TypeHandle.__Game_Citizens_HouseholdCitizen_RO_BufferTypeHandle;
        jobData.m_HouseholdAnimalType = __TypeHandle.__Game_Citizens_HouseholdAnimal_RO_BufferTypeHandle;
        jobData.m_OwnedVehicleType = __TypeHandle.__Game_Vehicles_OwnedVehicle_RO_BufferTypeHandle;
        jobData.m_HouseholdPets = __TypeHandle.__Game_Citizens_HouseholdPet_RO_ComponentLookup;
        jobData.m_Citizens = __TypeHandle.__Game_Citizens_Citizen_RO_ComponentLookup;
        jobData.m_Vehicles = __TypeHandle.__Game_Vehicles_Vehicle_RO_ComponentLookup;
        jobData.m_LayoutElements = __TypeHandle.__Game_Vehicles_LayoutElement_RO_BufferLookup;
        jobData.m_Renters = __TypeHandle.__Game_Buildings_Renter_RW_BufferLookup;
        jobData.m_RentEventArchetype = m_RentEventArchetype;
        jobData.m_CommandBuffer = m_ModificationBarrier.CreateCommandBuffer();
        JobHandle jobHandle = JobChunkExtensions.Schedule(jobData, m_HouseholdQuery, base.Dependency);
        m_ModificationBarrier.AddJobHandleForProducer(jobHandle);
        base.Dependency = jobHandle;
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
    public HouseholdRemoveSystem()
    {
    }
}
