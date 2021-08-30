using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class MoverSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation translation,ref MoveSpeedComponent moveSpeedComponent) => {

            //translation.Value.y += 1f * Time.DeltaTime;
            translation.Value.y += moveSpeedComponent.moveSpeed * Time.DeltaTime;
            
            if(translation.Value.y > 5f)
            {
                moveSpeedComponent.moveSpeed = -Mathf.Abs(moveSpeedComponent.moveSpeed);
            }

            if (translation.Value.y < -5f)
            {
                moveSpeedComponent.moveSpeed = +Mathf.Abs(moveSpeedComponent.moveSpeed);
            }


        });
    }
}
