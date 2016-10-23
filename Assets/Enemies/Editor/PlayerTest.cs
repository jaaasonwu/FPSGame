using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using UnityEngine.UI;


/* 
 * Created by Jiacheng Wu, jiachengw@student.unimelb.edu.au
 *
 * This class tests the behavior of player to make sure it's as expected
 */
public class PlayerTest {

    [Test]
    /*
     * Test if the resulting hp is right after being hit
     */
    public void PlayerOnHit()
    {
        Player player = new Player();
        player.hp = 100;
        player.OnHit(10);
        Assert.AreEqual(player.hp, 90);
    }

    [Test]
    /*
     * Test if the resulting is right after switching
     */
    public void SwitchWeaponTest()
    {
        GameObject playerObject = new GameObject();
        playerObject.AddComponent<Player>();

        // Create a fake ammotext
        GameObject text = new GameObject();
        UnityEngine.UI.Text ammoText = text.AddComponent<UnityEngine.UI.Text>();
        text.tag = "AmmoText";

        // initialize player
        Player player = playerObject.GetComponent<Player>();
        player.isLocal = true;
        player.weaponNumber = 0;
        player.ammo = 500;
        player.weapons = new GameObject[2];

        // initialize fake player and weapon object
        GameObject fakeWeapon = new GameObject();
        GameObject fakePlayer = new GameObject();
        fakePlayer.AddComponent<Player>();
        fakeWeapon.transform.parent = fakePlayer.transform;
        fakeWeapon.AddComponent<Weapon>();
        for (int i = 0; i < 2; i++)
        {
            player.weapons[i] = fakeWeapon;
        }
        player.ShowWeapon(player.weaponNumber);
        int weaponIndex = player.weaponNumber;
        
        // Switch weapon
        player.NextWeapon();
        if (weaponIndex == 2)
        {
            Assert.AreEqual(player.weaponNumber, 0);
        } else
        {
            Assert.AreEqual(player.weaponNumber, weaponIndex + 1);
        }
    }

    [Test]
    /*
     * Load function is used when loading game state
     */
    public void LoadTest()
    {
        Player player = new Player();
        Vector3 pos = new Vector3(0, 0, 0);
        Quaternion rot = Quaternion.identity;
        Messages.LoadPlayerMessage load = new Messages.LoadPlayerMessage(0,
            "jason", pos, rot, 1, 0, 100, 100, 0, 500);
        player.Load(load);
        Assert.AreEqual(player.id, 0);
        Assert.AreEqual(player.username, "jason");
        Assert.AreEqual(player.level, 1);
        Assert.AreEqual(player.exp, 0);
        Assert.AreEqual(player.hp, 100);
        Assert.AreEqual(player.maxHp, 100);
        Assert.AreEqual(player.weaponNumber, 0);
        Assert.AreEqual(player.ammo, 500);
    }

    [Test]
    /*
     * Test if the PlayerData class generated for saving is correct
     */
    public void GenerateDataTest()
    {
        GameObject playerObject = new GameObject();
        GameObject playerParent = new GameObject();
        playerParent.transform.position = new Vector3(0, 0, 0);
        playerParent.transform.rotation = Quaternion.identity;
        playerObject.AddComponent<Player>();
        Player player = playerObject.GetComponent<Player>();
        player.id = 0;
        player.username = "jason";
        playerObject.transform.parent = playerParent.transform;
        player.level = 1;
        player.exp = 0;
        player.hp = 100;
        player.maxHp = 100;
        player.weaponNumber = 0;
        player.isAttacking = false;
        player.isLocal = false;
        player.ammo = 500;

        PlayerData data = player.GeneratePlayerData();

        Assert.AreEqual(data.id, 0);
        Assert.AreEqual(data.username, "jason");
        Assert.AreEqual(data.pos, new Vector3(0, 0, 0));
        Assert.AreEqual(data.rot, Quaternion.identity);
        Assert.AreEqual(data.level, 1);
        Assert.AreEqual(data.exp, 0);
        Assert.AreEqual(data.hp, 100);
        Assert.AreEqual(data.maxHp, 100);
        Assert.AreEqual(data.isAttacking, false);
        Assert.AreEqual(data.isLocal, false);
        Assert.AreEqual(data.ammo, 500);

    }
}
