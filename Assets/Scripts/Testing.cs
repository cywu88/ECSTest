using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;

public class Testing : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;

    // Start is called before the first frame update
    void Start()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;



        //var entity = entityManager.CreateEntity(typeof(LevelComponent));

        EntityArchetype entityArchetype = entityManager.CreateArchetype(
           typeof(LevelComponent),
           typeof(Translation),
           typeof(RenderMesh), 
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
 
}
