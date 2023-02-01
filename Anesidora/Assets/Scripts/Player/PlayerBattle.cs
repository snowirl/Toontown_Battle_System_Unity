using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerBattle : NetworkBehaviour
{
    [SyncVar]
    public int hp, maxHp;

    [HideInInspector]
    public GameObject battleCell;
    public GameObject cogPrefab;
    System.Guid id;

    public void SendGagData(GagData gagData)
    {
        if(!isLocalPlayer) {return;}

        var newGagData = gagData;

        newGagData.whichToon = (int)this.gameObject.GetComponent<NetworkIdentity>().netId;

        print("Sending data: " + gagData.whichToon);

        battleCell.GetComponent<BattleCalculator>().CmdSelectGag(newGagData);
    }

    public void SpawnCog() // For test purposes only
    {
        if(!isLocalPlayer) {return;}

        CmdSpawnCog();
    }

    void Awake()
    {
        // NetworkClient.RegisterSpawnHandler(creatureAssetId, SpawnDelegate, UnSpawnDelegate);
        // NetworkClient.RegisterPrefab(cogPrefab);
    }

    public override void OnStartClient()
    {
        base.OnStartServer();

        if(!isLocalPlayer) {return;}

        
    }

    [Command]
    public void CmdSpawnCog()
    {
        var cog = Instantiate(cogPrefab, Vector3.one, Quaternion.identity);

        SceneManager.MoveGameObjectToScene(cog, this.gameObject.scene);

        NetworkServer.Spawn(cog);

        print("Spawned cog");

        TargetSpawnedCog(cog);
    }

    [TargetRpc]
    public void TargetSpawnedCog(GameObject cog)
    {
        if(battleCell == null) {return;}

        battleCell.GetComponent<BattleCell>().CmdAddCogPending(cog);
    }

    GameObject SpawnDelegate(Vector3 position, System.Guid assetId) 
    {
        return Instantiate(cogPrefab, Vector3.one, Quaternion.identity);
    }

    void UnSpawnDelegate(GameObject spawned) 
    {
        Destroy(spawned);
    }
}
