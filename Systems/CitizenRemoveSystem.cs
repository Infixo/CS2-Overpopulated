using System.Runtime.CompilerServices;
using Colossal.Collections;
using Game.Agents;
using Game.Buildings;
using Game.Common;
using Game.Creatures;
using Game.Tools;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Scripting;

namespace Game.Citizens;

[CompilerGenerated]
public class CitizenRemoveSystem : GameSystemBase
{
    [BurstCompile]
    private struct RemoveCitizenJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle m_EntityType;

        [ReadOnly]
        public ComponentTypeHandle<HouseholdMember> m_HouseholdMemberType;

        [ReadOnly]
        public ComponentTypeHandle<Student> m_StudentType;

        [ReadOnly]
        public ComponentTypeHandle<CurrentBuilding> m_CurrentBuildingType;

        [ReadOnly]
        public ComponentTypeHandle<CurrentTransport> m_CurrentTransportType;

        [ReadOnly]
        public ComponentTypeHandle<HasJobSeeker> m_HasJobSeekerType;

        [ReadOnly]
        public ComponentTypeHandle<HasSchoolSeeker> m_HasSchoolSeekerType;

        [ReadOnly]
        public ComponentLookup<CurrentTransport> m_CurrentTransportData;

        [ReadOnly]
        public ComponentLookup<HouseholdPet> m_HouseholdPetData;

        [ReadOnly]
        public ComponentLookup<Creature> m_CreatureData;

        public BufferLookup<HouseholdCitizen> m_HouseholdCitizens;

        public BufferLookup<Game.Buildings.Student> m_SchoolStudents;

        public BufferLookup<HouseholdAnimal> m_HouseholdAnimals;

        public BufferLookup<Patient> m_Patients;

        public BufferLookup<Occupant> m_Occupants;

        [ReadOnly]
        public ComponentLookup<Deleted> m_Deleteds;

        public EntityCommandBuffer m_CommandBuffer;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> nativeArray = chunk.GetNativeArray(m_EntityType);
            NativeArray<HasJobSeeker> nativeArray2 = chunk.GetNativeArray(ref m_HasJobSeekerType);
            NativeArray<HasSchoolSeeker> nativeArray3 = chunk.GetNativeArray(ref m_HasSchoolSeekerType);
            NativeArray<HouseholdMember> nativeArray4 = chunk.GetNativeArray(ref m_HouseholdMemberType);
            NativeArray<Student> nativeArray5 = chunk.GetNativeArray(ref m_StudentType);
            for (int i = 0; i < nativeArray4.Length; i++)
            {
                Entity citizen = nativeArray[i];
                HouseholdMember householdMember = nativeArray4[i];
                if (m_HouseholdCitizens.HasBuffer(householdMember.m_Household))
                {
                    DynamicBuffer<HouseholdCitizen> buffer = m_HouseholdCitizens[householdMember.m_Household];
                    CollectionUtils.RemoveValue(buffer, new HouseholdCitizen(citizen));
                    if (buffer.Length == 0)
                    {
                        m_CommandBuffer.AddComponent(householdMember.m_Household, default(Deleted));
                    }
                }
                if (nativeArray2.IsCreated)
                {
                    Entity seeker = nativeArray2[i].m_Seeker;
                    if (!m_Deleteds.HasComponent(seeker))
                    {
                        m_CommandBuffer.AddComponent(seeker, default(Deleted));
                    }
                }
                if (nativeArray3.IsCreated)
                {
                    Entity seeker2 = nativeArray3[i].m_Seeker;
                    if (!m_Deleteds.HasComponent(seeker2))
                    {
                        m_CommandBuffer.AddComponent(seeker2, default(Deleted));
                    }
                }
                if (nativeArray5.IsCreated && m_SchoolStudents.HasBuffer(nativeArray5[i].m_School))
                {
                    CollectionUtils.RemoveValue(m_SchoolStudents[nativeArray5[i].m_School], new Game.Buildings.Student(nativeArray[i]));
                }
            }
            NativeArray<CurrentBuilding> nativeArray6 = chunk.GetNativeArray(ref m_CurrentBuildingType);
            for (int j = 0; j < nativeArray6.Length; j++)
            {
                CurrentBuilding currentBuilding = nativeArray6[j];
                if (m_Patients.HasBuffer(currentBuilding.m_CurrentBuilding))
                {
                    CollectionUtils.RemoveValue(m_Patients[currentBuilding.m_CurrentBuilding], new Patient(nativeArray[j]));
                }
                if (m_Occupants.HasBuffer(currentBuilding.m_CurrentBuilding))
                {
                    CollectionUtils.RemoveValue(m_Occupants[currentBuilding.m_CurrentBuilding], new Occupant(nativeArray[j]));
                }
            }
            NativeArray<CurrentTransport> nativeArray7 = chunk.GetNativeArray(ref m_CurrentTransportType);
            for (int k = 0; k < nativeArray7.Length; k++)
            {
                CurrentTransport currentTransport = nativeArray7[k];
                if (m_CreatureData.HasComponent(currentTransport.m_CurrentTransport))
                {
                    m_CommandBuffer.AddComponent(currentTransport.m_CurrentTransport, default(Deleted));
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
        public ComponentTypeHandle<HouseholdMember> __Game_Citizens_HouseholdMember_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<CurrentBuilding> __Game_Citizens_CurrentBuilding_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<CurrentTransport> __Game_Citizens_CurrentTransport_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<HasJobSeeker> __Game_Agents_HasJobSeeker_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<HasSchoolSeeker> __Game_Citizens_HasSchoolSeeker_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<Student> __Game_Citizens_Student_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentLookup<CurrentTransport> __Game_Citizens_CurrentTransport_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<HouseholdPet> __Game_Citizens_HouseholdPet_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Creature> __Game_Creatures_Creature_RO_ComponentLookup;

        public BufferLookup<HouseholdCitizen> __Game_Citizens_HouseholdCitizen_RW_BufferLookup;

        public BufferLookup<HouseholdAnimal> __Game_Citizens_HouseholdAnimal_RW_BufferLookup;

        public BufferLookup<Game.Buildings.Student> __Game_Buildings_Student_RW_BufferLookup;

        public BufferLookup<Patient> __Game_Buildings_Patient_RW_BufferLookup;

        public BufferLookup<Occupant> __Game_Buildings_Occupant_RW_BufferLookup;

        [ReadOnly]
        public ComponentLookup<Deleted> __Game_Common_Deleted_RO_ComponentLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
            __Game_Citizens_HouseholdMember_RO_ComponentTypeHandle = state.GetComponentTypeHandle<HouseholdMember>(isReadOnly: true);
            __Game_Citizens_CurrentBuilding_RO_ComponentTypeHandle = state.GetComponentTypeHandle<CurrentBuilding>(isReadOnly: true);
            __Game_Citizens_CurrentTransport_RO_ComponentTypeHandle = state.GetComponentTypeHandle<CurrentTransport>(isReadOnly: true);
            __Game_Agents_HasJobSeeker_RO_ComponentTypeHandle = state.GetComponentTypeHandle<HasJobSeeker>(isReadOnly: true);
            __Game_Citizens_HasSchoolSeeker_RO_ComponentTypeHandle = state.GetComponentTypeHandle<HasSchoolSeeker>(isReadOnly: true);
            __Game_Citizens_Student_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Student>(isReadOnly: true);
            __Game_Citizens_CurrentTransport_RO_ComponentLookup = state.GetComponentLookup<CurrentTransport>(isReadOnly: true);
            __Game_Citizens_HouseholdPet_RO_ComponentLookup = state.GetComponentLookup<HouseholdPet>(isReadOnly: true);
            __Game_Creatures_Creature_RO_ComponentLookup = state.GetComponentLookup<Creature>(isReadOnly: true);
            __Game_Citizens_HouseholdCitizen_RW_BufferLookup = state.GetBufferLookup<HouseholdCitizen>();
            __Game_Citizens_HouseholdAnimal_RW_BufferLookup = state.GetBufferLookup<HouseholdAnimal>();
            __Game_Buildings_Student_RW_BufferLookup = state.GetBufferLookup<Game.Buildings.Student>();
            __Game_Buildings_Patient_RW_BufferLookup = state.GetBufferLookup<Patient>();
            __Game_Buildings_Occupant_RW_BufferLookup = state.GetBufferLookup<Occupant>();
            __Game_Common_Deleted_RO_ComponentLookup = state.GetComponentLookup<Deleted>(isReadOnly: true);
        }
    }

    private EntityQuery m_CitizenQuery;

    private ModificationBarrier4 m_ModificationBarrier;

    private TypeHandle __TypeHandle;

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();
        m_ModificationBarrier = base.World.GetOrCreateSystemManaged<ModificationBarrier4>();
        m_CitizenQuery = GetEntityQuery(ComponentType.ReadOnly<Citizen>(), ComponentType.ReadOnly<Deleted>(), ComponentType.Exclude<Temp>());
        RequireForUpdate(m_CitizenQuery);
    }

    [Preserve]
    protected override void OnUpdate()
    {
        __TypeHandle.__Game_Common_Deleted_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_Occupant_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_Patient_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_Student_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdAnimal_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdCitizen_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Creatures_Creature_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdPet_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_CurrentTransport_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HasSchoolSeeker_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Agents_HasJobSeeker_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_CurrentTransport_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_CurrentBuilding_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
        RemoveCitizenJob jobData = default(RemoveCitizenJob);
        jobData.m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle;
        jobData.m_HouseholdMemberType = __TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentTypeHandle;
        jobData.m_CurrentBuildingType = __TypeHandle.__Game_Citizens_CurrentBuilding_RO_ComponentTypeHandle;
        jobData.m_CurrentTransportType = __TypeHandle.__Game_Citizens_CurrentTransport_RO_ComponentTypeHandle;
        jobData.m_HasJobSeekerType = __TypeHandle.__Game_Agents_HasJobSeeker_RO_ComponentTypeHandle;
        jobData.m_HasSchoolSeekerType = __TypeHandle.__Game_Citizens_HasSchoolSeeker_RO_ComponentTypeHandle;
        jobData.m_StudentType = __TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle;
        jobData.m_CurrentTransportData = __TypeHandle.__Game_Citizens_CurrentTransport_RO_ComponentLookup;
        jobData.m_HouseholdPetData = __TypeHandle.__Game_Citizens_HouseholdPet_RO_ComponentLookup;
        jobData.m_CreatureData = __TypeHandle.__Game_Creatures_Creature_RO_ComponentLookup;
        jobData.m_HouseholdCitizens = __TypeHandle.__Game_Citizens_HouseholdCitizen_RW_BufferLookup;
        jobData.m_HouseholdAnimals = __TypeHandle.__Game_Citizens_HouseholdAnimal_RW_BufferLookup;
        jobData.m_SchoolStudents = __TypeHandle.__Game_Buildings_Student_RW_BufferLookup;
        jobData.m_Patients = __TypeHandle.__Game_Buildings_Patient_RW_BufferLookup;
        jobData.m_Occupants = __TypeHandle.__Game_Buildings_Occupant_RW_BufferLookup;
        jobData.m_Deleteds = __TypeHandle.__Game_Common_Deleted_RO_ComponentLookup;
        jobData.m_CommandBuffer = m_ModificationBarrier.CreateCommandBuffer();
        JobHandle jobHandle = JobChunkExtensions.Schedule(jobData, m_CitizenQuery, base.Dependency);
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

    [Preserve]
    public CitizenRemoveSystem()
    {
    }
}
