using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
   private void OnCollisionEnter(Collision collision)
   {
      DataStorage.instance.DecreaseHealth(1);
   }
}
