using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameTileContentType{
    Empty,Destination
}

public class GameTileContent : MonoBehaviour
{
   [SerializeField]
   GameTileContentType type=default;

   GameTileContentFactory originFactory;

   public GameTileContentFactory OriginFactory{
       get=>originFactory;
       set{
           Debug.Assert(originFactory==null,"Redefined origin factory!");
           originFactory=value;
       }
   }

   public void Recycle(){
       originFactory.Reclaim(this);
   }

   public GameTileContentType Type=>type;
}
