using System.Runtime.CompilerServices;
using Game.Buildings;
using Game.City;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Scripting;

namespace Game.Serialization;

[CompilerGenerated]
public class RenterSystem : GameSystemBase
{
    [BurstCompile]
    private struct RenterJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle m_EntityType;

        [ReadOnly]
        public ComponentTypeHandle<PropertyRenter> m_PropertyRenterType;

        [ReadOnly]
        public ComponentLookup<CityServiceUpkeep> m_ServiceBuildings;

        public BufferLookup<Renter> m_Renters;

        public EntityCommandBuffer m_CommandBuffer;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> nativeArray = chunk.GetNativeArray(m_EntityType);
            NativeArray<PropertyRenter> nativeArray2 = chunk.GetNativeArray(ref m_PropertyRenterType);
            for (int i = 0; i < nativeArray2.Length; i++)
            {
                Entity entity = nativeArray[i];
                PropertyRenter propertyRenter = nativeArray2[i];
                if (m_Renters.HasBuffer(propertyRenter.m_Property))
                {
                    m_Renters[propertyRenter.m_Property].Add(new Renter
                    {
                        m_Renter = entity
                    });
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
            __Game_Buildings_PropertyRenter_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PropertyRenter>(isReadOnly: true);
            __Game_Buildings_Renter_RW_BufferLookup = state.GetBufferLookup<Renter>();
            __Game_City_CityServiceUpkeep_RO_ComponentLookup = state.GetComponentLookup<CityServiceUpkeep>(isReadOnly: true);
        }
    }

    private DeserializationBarrier m_DeserializationBarrier;

    private EntityQuery m_Query;

    private TypeHandle __TypeHandle;

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();
        m_DeserializationBarrier = base.World.GetOrCreateSystemManaged<DeserializationBarrier>();
        m_Query = GetEntityQuery(ComponentType.ReadOnly<PropertyRenter>());
        RequireForUpdate(m_Query);
    }

    [Preserve]
    protected override void OnUpdate()
    {
        __TypeHandle.__Game_City_CityServiceUpkeep_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_Renter_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
        RenterJob renterJob = default(RenterJob);
        renterJob.m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle;
        renterJob.m_PropertyRenterType = __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle;
        renterJob.m_Renters = __TypeHandle.__Game_Buildings_Renter_RW_BufferLookup;
        renterJob.m_ServiceBuildings = __TypeHandle.__Game_City_CityServiceUpkeep_RO_ComponentLookup;
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

    [Preserve]
    public RenterSystem()
    {
    }
}
