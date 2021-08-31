using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class Testing : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;


    [SerializeField] private Transform pfZombie;
    private List<Zombie> zombieList;

    public class Zombie
    {
        public Transform transform; 
        public float moveY;
    }




    // Start is called before the first frame update
    void Start()
    {

        zombieList = new List<Zombie>();
        for (int i = 0; i < 1000; i++)
        {
            Transform zombieTransform = Instantiate(pfZombie, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);
            zombieList.Add(new Zombie
            {
                transform = zombieTransform,
                moveY = UnityEngine.Random.Range(1f, 2f)
            }); 
        }

        return;




        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;


        //var entity = entityManager.CreateEntity(typeof(LevelComponent));

        EntityArchetype entityArchetype = entityManager.CreateArchetype(
           typeof(LevelComponent),
           typeof(Translation),
           typeof(RenderMesh),
           //https://answers.unity.com/questions/1701725/ecs-rendermesh-not-work.html
           typeof(RenderBounds),   
           typeof(LocalToWorld),
           typeof(MoveSpeedComponent)
        );

        //var entity = entityManager.CreateEntity(entityArchetype);
        //entityManager.SetComponentData(entity, new LevelComponent { level = 10 });

        NativeArray<Entity> entityArray = new NativeArray<Entity>(10, Allocator.Temp);


        entityManager.CreateEntity(entityArchetype, entityArray);
        for (int i = 0; i < entityArray.Length; i++)
        {
            var entity = entityArray[i];

            entityManager.SetComponentData(entity, 
                new LevelComponent {
                    level = UnityEngine.Random.Range(10,20)
                }
            );

            entityManager.SetComponentData(entity,
                new MoveSpeedComponent {
                    moveSpeed = UnityEngine.Random.Range(1f, 2f)
                }
            );
            entityManager.SetComponentData(entity, 
                new Translation {
                    Value = new float3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f), 0)
                }
            );

            entityManager.SetSharedComponentData(entity, 
                new RenderMesh  {
                    mesh = mesh,
                    material = material, 
                });
        }


        entityArray.Dispose();

    }


    [SerializeField] private bool useJobs;

    private void jobtest2()
    {
        float startTime = Time.realtimeSinceStartup;

        NativeArray<float3> positionArray = new NativeArray<float3>(zombieList.Count, Allocator.TempJob);
        NativeArray<float> moveYarry = new NativeArray<float>(zombieList.Count, Allocator.TempJob);

        for (int i = 0; i < zombieList.Count; i++)
        {
            positionArray[i] = zombieList[i].transform.position;
            moveYarry[i] = zombieList[i].moveY;

        }
 
        ReallyToughParalle1Job reallyToughParalle1Job = new ReallyToughParalle1Job
        {
            deltaTime = Time.deltaTime,
            positionArray = positionArray,
            moveYArray = moveYarry,
        };

        JobHandle jobHandle = reallyToughParalle1Job.Schedule(zombieList.Count, 100);
        jobHandle.Complete();
         
        for (int i = 0; i < zombieList.Count; i++)
        {
            zombieList[i].transform.position = positionArray[i];
            zombieList[i].moveY = moveYarry[i];
        }

        positionArray.Dispose();
        moveYarry.Dispose();
    }

    private void jobtest3()
    {
        float startTime = Time.realtimeSinceStartup;

        //NativeArray<float3> positionArray = new NativeArray<float3>(zombieList.Count, Allocator.TempJob);
        NativeArray<float> moveYArray = new NativeArray<float>(zombieList.Count, Allocator.TempJob);
        TransformAccessArray transformAccessArray = new TransformAccessArray(zombieList.Count);


        for (int i = 0; i < zombieList.Count; i++)
        {
            //positionArray[i] = zombieList[i].transform.position;
            moveYArray[i] = zombieList[i].moveY;
            transformAccessArray.Add(zombieList[i].transform);
        }

        /*
        ReallyToughParalle1Job reallyToughParalle1Job = new ReallyToughParalle1Job
        {
            deltaTime = Time.deltaTime,
            positionArray = positionArray,
            moveYArray = moveYarry,
        };

        JobHandle jobHandle = reallyToughParalle1Job.Schedule(zombieList.Count, 100);
        jobHandle.Complete();
        */


        ReallyToughParalle1JobTransforms reallyToughParalle1JobTransforms = new ReallyToughParalle1JobTransforms
        {
            deltaTime = Time.deltaTime,
            moveYArray = moveYArray,
        };
        JobHandle jobHandle = reallyToughParalle1JobTransforms.Schedule(transformAccessArray);

        for (int i = 0; i < zombieList.Count; i++)
        {
            //zombieList[i].transform.position = positionArray[i];
            zombieList[i].moveY = moveYArray[i];

        }

        //positionArray.Dispose();
        moveYArray.Dispose();
        transformAccessArray.Dispose();
    }


    private void Update()
    {
        float startTime = Time.realtimeSinceStartup;
        if(useJobs)
        {
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);

            for (int i = 0; i < 10; i++)
            {
                JobHandle jobHandle = ReallyToughTaskJob();
                jobHandleList.Add(jobHandle);
                //jobHandle.Complete();
            }
            JobHandle.CompleteAll(jobHandleList);

            jobHandleList.Dispose(); 
        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                ReallyToughTask();
            } 
        }
        

        Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms"); 
    }


    private void ReallyToughTask()
    {
        float value = 0f;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
         
    }

    private JobHandle ReallyToughTaskJob()
    {
        ReallyToughJob job = new ReallyToughJob();
        return job.Schedule();

    }

    public struct ReallyToughJob : IJob
    {
 
        public void Execute()
        {
            float value = 0f;
            for (int i = 0; i < 50000; i++)
            {
                value = math.exp10(math.sqrt(value));
            }
        }
    }

    [BurstCompile]
    public struct ReallyToughParalle1Job : IJobParallelFor
    {
        public NativeArray<float3> positionArray;

        public NativeArray<float> moveYArray;

        [ReadOnly] public float deltaTime;
         
        public void Execute(int index)
        {
            positionArray[index] += new float3(0, moveYArray[index] * Time.deltaTime, 0f);

            if(positionArray[index].y > 5f)
            {
                moveYArray[index] = -math.abs(moveYArray[index]);
            }

            if(positionArray[index].y < -5f)
            {
                moveYArray[index] = +math.abs(moveYArray[index]);
            }

            float value = 0f;
            for (int i = 0; i < 1000; i++)
            {
                value = math.exp10(math.sqrt(value));
            }
        } 
    }

    public struct ReallyToughParalle1JobTransforms : IJobParallelForTransform
    { 
        public NativeArray<float3> positionArray;

        public NativeArray<float> moveYArray;

        [ReadOnly] public float deltaTime;

        public void Execute(int index, TransformAccess transform)
        {
            transform.position += new Vector3(0, moveYArray[index] * Time.deltaTime, 0f);

            if (transform.position.y > 5f)
            {
                moveYArray[index] = -math.abs(moveYArray[index]);
            }

            if (transform.position.y < -5f)
            {
                moveYArray[index] = +math.abs(moveYArray[index]);
            }

            float value = 0f;
            for (int i = 0; i < 1000; i++)
            {
                value = math.exp10(math.sqrt(value));
            }
        }
    }

}
