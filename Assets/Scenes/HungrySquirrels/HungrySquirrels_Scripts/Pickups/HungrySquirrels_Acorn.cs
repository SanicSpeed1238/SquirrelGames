using UnityEngine;

public class HungrySquirrels_Acorn : Pickup
{
    [SerializeField] private int _pointVal = 1;
    protected override void Collect(HungrySquirrels_ArmCollision arm)
    {
        Debug.Log(_pointVal);//Acorn is now within player's possession.
        //Destroy(this.gameObject);
    }
    //When the Acorn hits the grabber, it will give the player a point
    /*public void AcornCollected(HungrySquirrels_PlayerController player)
    {
        player.collided = true;

        Destroy(gameObject);
    }*/
}
