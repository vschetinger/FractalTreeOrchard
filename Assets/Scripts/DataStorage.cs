using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataStorage : MonoBehaviour
{
   public static DataStorage instance;
    
   public int score
   {
      get;
      private set;
   }
   [field: SerializeField]
   public int health
   {
      get;
      private set;
   }

   public void Start()
   {
      if (instance == null)
         instance = this;
   }

   public void IncreaseScore(int increaseBy)
   {
      score += increaseBy;
      Debug.Log("Score: " + score);
   }
   
   public void DecreaseHealth(int decreaseBy)
   {
      health -= decreaseBy;
      Debug.Log("Health: " + health);
   }
   
}
