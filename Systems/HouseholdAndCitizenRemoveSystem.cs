using System.Runtime.CompilerServices;
using Colossal.Collections;
using Game.Agents;
using Game.Buildings;
using Game.Common;
using Game.Creatures;
using Game.Tools;
using Game.Vehicles;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Scripting;

namespace Game.Citizens;

[CompilerGenerated]
public class HouseholdAndCitizenRemoveSystem : GameSystemBase
{
    [BurstCompile]
    private struct HouseholdAndCitizenRemoveJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle m_EntityType;

        [ReadOnly]
        public ComponentTypeHandle<Citizen> m_CitizenType;

        [ReadOnly]
        public ComponentLookup<HouseholdPet> m_HouseholdPets;

        [ReadOnly]
        public ComponentLookup<HasSchoolSeeker> m_HasSchoolSeekers;

        [ReadOnly]
        public ComponentLookup<HouseholdMember> m_HouseholdMembers;

        [ReadOnly]
        public ComponentLookup<PropertyRenter> m_PropertyRenters;

        [ReadOnly]
        public ComponentLookup<HasJobSeeker> m_HasJobSeekers;

        [ReadOnly]
        public ComponentLookup<Citizen> m_Citizens;

        [ReadOnly]
        public ComponentLookup<Student> m_Students;

        [ReadOnly]
        public ComponentLookup<CurrentBuilding> m_CurrentBuildings;

        [ReadOnly]
        public ComponentLookup<CurrentTransport> m_CurrentTransports;

        [ReadOnly]
        public ComponentLookup<Creature> m_Creatures;

        [ReadOnly]
        public ComponentLookup<Vehicle> m_Vehicles;

        [ReadOnly]
        public ComponentLookup<Deleted> m_Deleteds;

        [ReadOnly]
        public BufferLookup<OwnedVehicle> m_OwnedVehicles;

        [ReadOnly]
        public BufferLookup<LayoutElement> m_LayoutElements;

        public BufferLookup<Game.Buildings.Student> m_SchoolStudents;

        public BufferLookup<HouseholdCitizen> m_HouseholdCitizens;

        public BufferLookup<HouseholdAnimal> m_HouseholdAnimals;

        public BufferLookup<Patient> m_Patients;

        public BufferLookup<Occupant> m_Occupants;

        public BufferLookup<Renter> m_Renters;

        [ReadOnly]
        public EntityArchetype m_RentEventArchetype;

        public EntityCommandBuffer m_CommandBuffer;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> nativeArray = chunk.GetNativeArray(m_EntityType);
            if (chunk.Has(ref m_CitizenType))
            {
                for (int i = 0; i < nativeArray.Length; i++)
                {
                    Entity entity = nativeArray[i];
                    RemoveCitizen(entity);
                    HouseholdMember householdMember = m_HouseholdMembers[entity];
                    if (m_HouseholdCitizens.HasBuffer(householdMember.m_Household))
                    {
                        DynamicBuffer<HouseholdCitizen> buffer = m_HouseholdCitizens[householdMember.m_Household];
                        CollectionUtils.RemoveValue(buffer, new HouseholdCitizen(entity));
                        if (buffer.Length == 0)
                        {
                            RemoveHousehold(householdMember.m_Household);
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < nativeArray.Length; j++)
                {
                    Entity entity2 = nativeArray[j];
                    RemoveHousehold(entity2);
                }
            }
        }

        private void RemoveCitizen(Entity entity)
        {
            if (!m_Deleteds.HasComponent(entity))
            {
                m_CommandBuffer.AddComponent(entity, default(Deleted));
            }
            if (m_HasJobSeekers.HasComponent(entity))
            {
                Entity seeker = m_HasJobSeekers[entity].m_Seeker;
                if (!m_Deleteds.HasComponent(seeker))
                {
                    m_CommandBuffer.AddComponent(seeker, default(Deleted));
                }
            }
            if (m_HasSchoolSeekers.HasComponent(entity))
            {
                Entity seeker2 = m_HasSchoolSeekers[entity].m_Seeker;
                if (!m_Deleteds.HasComponent(seeker2))
                {
                    m_CommandBuffer.AddComponent(seeker2, default(Deleted));
                }
            }
            if (m_Students.HasComponent(entity) && m_SchoolStudents.HasBuffer(m_Students[entity].m_School))
            {
                CollectionUtils.RemoveValue(m_SchoolStudents[m_Students[entity].m_School], new Game.Buildings.Student(entity));
            }
            if (m_CurrentBuildings.HasComponent(entity))
            {
                CurrentBuilding currentBuilding = m_CurrentBuildings[entity];
                if (m_Patients.HasBuffer(currentBuilding.m_CurrentBuilding))
                {
                    CollectionUtils.RemoveValue(m_Patients[currentBuilding.m_CurrentBuilding], new Patient(entity));
                }
                if (m_Occupants.HasBuffer(currentBuilding.m_CurrentBuilding))
                {
                    CollectionUtils.RemoveValue(m_Occupants[currentBuilding.m_CurrentBuilding], new Occupant(entity));
                }
            }
            if (m_CurrentTransports.HasComponent(entity))
            {
                CurrentTransport currentTransport = m_CurrentTransports[entity];
                if (m_Creatures.HasComponent(currentTransport.m_CurrentTransport) && !m_Deleteds.HasComponent(currentTransport.m_CurrentTransport))
                {
                    m_CommandBuffer.AddComponent(currentTransport.m_CurrentTransport, default(Deleted));
                }
            }
        }

        private void RemoveHousehold(Entity entity)
        {
            if (m_HouseholdAnimals.HasBuffer(entity))
            {
                DynamicBuffer<HouseholdAnimal> dynamicBuffer = m_HouseholdAnimals[entity];
                for (int i = 0; i < dynamicBuffer.Length; i++)
                {
                    if (m_HouseholdPets.HasComponent(dynamicBuffer[i].m_HouseholdPet))
                    {
                        m_CommandBuffer.AddComponent(dynamicBuffer[i].m_HouseholdPet, default(Deleted));
                    }
                }
            }
            if (m_HouseholdCitizens.HasBuffer(entity))
            {
                DynamicBuffer<HouseholdCitizen> dynamicBuffer2 = m_HouseholdCitizens[entity];
                for (int j = 0; j < dynamicBuffer2.Length; j++)
                {
                    if (m_Citizens.HasComponent(dynamicBuffer2[j].m_Citizen))
                    {
                        RemoveCitizen(dynamicBuffer2[j].m_Citizen);
                    }
                }
            }
            if (m_OwnedVehicles.HasBuffer(entity))
            {
                DynamicBuffer<OwnedVehicle> dynamicBuffer3 = m_OwnedVehicles[entity];
                for (int k = 0; k < dynamicBuffer3.Length; k++)
                {
                    Entity vehicle = dynamicBuffer3[k].m_Vehicle;
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
            if (!m_PropertyRenters.HasComponent(entity))
            {
                return;
            }
            PropertyRenter propertyRenter = m_PropertyRenters[entity];
            if (!m_Renters.TryGetBuffer(propertyRenter.m_Property, out var bufferData))
            {
                return;
            }
            for (int l = 0; l < bufferData.Length; l++)
            {
                if (bufferData[l].m_Renter == entity)
                {
                    bufferData.RemoveAt(l);
                    break;
                }
            }
            Entity e = m_CommandBuffer.CreateEntity(m_RentEventArchetype);
            m_CommandBuffer.SetComponent(e, new RentersUpdated(propertyRenter.m_Property));
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
        public ComponentTypeHandle<Citizen> __Game_Citizens_Citizen_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentLookup<HouseholdPet> __Game_Citizens_HouseholdPet_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<HasSchoolSeeker> __Game_Citizens_HasSchoolSeeker_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<HouseholdMember> __Game_Citizens_HouseholdMember_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<HasJobSeeker> __Game_Agents_HasJobSeeker_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Citizen> __Game_Citizens_Citizen_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Student> __Game_Citizens_Student_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Vehicle> __Game_Vehicles_Vehicle_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<CurrentBuilding> __Game_Citizens_CurrentBuilding_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<CurrentTransport> __Game_Citizens_CurrentTransport_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Creature> __Game_Creatures_Creature_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Deleted> __Game_Common_Deleted_RO_ComponentLookup;

        [ReadOnly]
        public BufferLookup<OwnedVehicle> __Game_Vehicles_OwnedVehicle_RO_BufferLookup;

        [ReadOnly]
        public BufferLookup<LayoutElement> __Game_Vehicles_LayoutElement_RO_BufferLookup;

        public BufferLookup<Game.Buildings.Student> __Game_Buildings_Student_RW_BufferLookup;

        public BufferLookup<HouseholdCitizen> __Game_Citizens_HouseholdCitizen_RW_BufferLookup;

        public BufferLookup<HouseholdAnimal> __Game_Citizens_HouseholdAnimal_RW_BufferLookup;

        public BufferLookup<Patient> __Game_Buildings_Patient_RW_BufferLookup;

        public BufferLookup<Occupant> __Game_Buildings_Occupant_RW_BufferLookup;

        public BufferLookup<Renter> __Game_Buildings_Renter_RW_BufferLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
            __Game_Citizens_Citizen_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Citizen>(isReadOnly: true);
            __Game_Citizens_HouseholdPet_RO_ComponentLookup = state.GetComponentLookup<HouseholdPet>(isReadOnly: true);
            __Game_Citizens_HasSchoolSeeker_RO_ComponentLookup = state.GetComponentLookup<HasSchoolSeeker>(isReadOnly: true);
            __Game_Citizens_HouseholdMember_RO_ComponentLookup = state.GetComponentLookup<HouseholdMember>(isReadOnly: true);
            __Game_Buildings_PropertyRenter_RO_ComponentLookup = state.GetComponentLookup<PropertyRenter>(isReadOnly: true);
            __Game_Agents_HasJobSeeker_RO_ComponentLookup = state.GetComponentLookup<HasJobSeeker>(isReadOnly: true);
            __Game_Citizens_Citizen_RO_ComponentLookup = state.GetComponentLookup<Citizen>(isReadOnly: true);
            __Game_Citizens_Student_RO_ComponentLookup = state.GetComponentLookup<Student>(isReadOnly: true);
            __Game_Vehicles_Vehicle_RO_ComponentLookup = state.GetComponentLookup<Vehicle>(isReadOnly: true);
            __Game_Citizens_CurrentBuilding_RO_ComponentLookup = state.GetComponentLookup<CurrentBuilding>(isReadOnly: true);
            __Game_Citizens_CurrentTransport_RO_ComponentLookup = state.GetComponentLookup<CurrentTransport>(isReadOnly: true);
            __Game_Creatures_Creature_RO_ComponentLookup = state.GetComponentLookup<Creature>(isReadOnly: true);
            __Game_Common_Deleted_RO_ComponentLookup = state.GetComponentLookup<Deleted>(isReadOnly: true);
            __Game_Vehicles_OwnedVehicle_RO_BufferLookup = state.GetBufferLookup<OwnedVehicle>(isReadOnly: true);
            __Game_Vehicles_LayoutElement_RO_BufferLookup = state.GetBufferLookup<LayoutElement>(isReadOnly: true);
            __Game_Buildings_Student_RW_BufferLookup = state.GetBufferLookup<Game.Buildings.Student>();
            __Game_Citizens_HouseholdCitizen_RW_BufferLookup = state.GetBufferLookup<HouseholdCitizen>();
            __Game_Citizens_HouseholdAnimal_RW_BufferLookup = state.GetBufferLookup<HouseholdAnimal>();
            __Game_Buildings_Patient_RW_BufferLookup = state.GetBufferLookup<Patient>();
            __Game_Buildings_Occupant_RW_BufferLookup = state.GetBufferLookup<Occupant>();
            __Game_Buildings_Renter_RW_BufferLookup = state.GetBufferLookup<Renter>();
        }
    }

    private EntityQuery m_DeletedQuery;

    private EntityArchetype m_RentEventArchetype;

    private ModificationBarrier2 m_ModificationBarrier;

    private TypeHandle __TypeHandle;

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();
        m_ModificationBarrier = base.World.GetOrCreateSystemManaged<ModificationBarrier2>();
        m_DeletedQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[1] { ComponentType.ReadOnly<Deleted>() },
            Any = new ComponentType[2]
            {
                ComponentType.ReadOnly<Household>(),
                ComponentType.ReadOnly<Citizen>()
            },
            None = new ComponentType[1] { ComponentType.ReadOnly<Temp>() }
        });
        m_RentEventArchetype = base.EntityManager.CreateArchetype(ComponentType.ReadWrite<Event>(), ComponentType.ReadWrite<RentersUpdated>());
        RequireForUpdate(m_DeletedQuery);
    }

    [Preserve]
    protected override void OnUpdate()
    {
        __TypeHandle.__Game_Buildings_Renter_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_Occupant_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_Patient_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdAnimal_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdCitizen_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_Student_RW_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Vehicles_LayoutElement_RO_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Vehicles_OwnedVehicle_RO_BufferLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Common_Deleted_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Creatures_Creature_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_CurrentTransport_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_CurrentBuilding_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Vehicles_Vehicle_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Student_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Citizen_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Agents_HasJobSeeker_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HasSchoolSeeker_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdPet_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Citizen_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
        HouseholdAndCitizenRemoveJob jobData = default(HouseholdAndCitizenRemoveJob);
        jobData.m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle;
        jobData.m_CitizenType = __TypeHandle.__Game_Citizens_Citizen_RO_ComponentTypeHandle;
        jobData.m_HouseholdPets = __TypeHandle.__Game_Citizens_HouseholdPet_RO_ComponentLookup;
        jobData.m_HasSchoolSeekers = __TypeHandle.__Game_Citizens_HasSchoolSeeker_RO_ComponentLookup;
        jobData.m_HouseholdMembers = __TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentLookup;
        jobData.m_PropertyRenters = __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup;
        jobData.m_HasJobSeekers = __TypeHandle.__Game_Agents_HasJobSeeker_RO_ComponentLookup;
        jobData.m_Citizens = __TypeHandle.__Game_Citizens_Citizen_RO_ComponentLookup;
        jobData.m_Students = __TypeHandle.__Game_Citizens_Student_RO_ComponentLookup;
        jobData.m_Vehicles = __TypeHandle.__Game_Vehicles_Vehicle_RO_ComponentLookup;
        jobData.m_CurrentBuildings = __TypeHandle.__Game_Citizens_CurrentBuilding_RO_ComponentLookup;
        jobData.m_CurrentTransports = __TypeHandle.__Game_Citizens_CurrentTransport_RO_ComponentLookup;
        jobData.m_Creatures = __TypeHandle.__Game_Creatures_Creature_RO_ComponentLookup;
        jobData.m_Deleteds = __TypeHandle.__Game_Common_Deleted_RO_ComponentLookup;
        jobData.m_OwnedVehicles = __TypeHandle.__Game_Vehicles_OwnedVehicle_RO_BufferLookup;
        jobData.m_LayoutElements = __TypeHandle.__Game_Vehicles_LayoutElement_RO_BufferLookup;
        jobData.m_SchoolStudents = __TypeHandle.__Game_Buildings_Student_RW_BufferLookup;
        jobData.m_HouseholdCitizens = __TypeHandle.__Game_Citizens_HouseholdCitizen_RW_BufferLookup;
        jobData.m_HouseholdAnimals = __TypeHandle.__Game_Citizens_HouseholdAnimal_RW_BufferLookup;
        jobData.m_Patients = __TypeHandle.__Game_Buildings_Patient_RW_BufferLookup;
        jobData.m_Occupants = __TypeHandle.__Game_Buildings_Occupant_RW_BufferLookup;
        jobData.m_Renters = __TypeHandle.__Game_Buildings_Renter_RW_BufferLookup;
        jobData.m_RentEventArchetype = m_RentEventArchetype;
        jobData.m_CommandBuffer = m_ModificationBarrier.CreateCommandBuffer();
        JobHandle jobHandle = JobChunkExtensions.Schedule(jobData, m_DeletedQuery, base.Dependency);
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
    public HouseholdAndCitizenRemoveSystem()
    {
    }
}
