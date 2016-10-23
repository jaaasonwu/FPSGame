using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class EnemyTest
{
    GameObject enemyObject = new GameObject ();
    GameObject playerObject = new GameObject ();
    Enemy enemy;
    Player player;

    public EnemyTest ()
    {
        enemy = enemyObject.AddComponent<Enemy> ();
        enemyObject.AddComponent<Rigidbody> ();
        player = playerObject.AddComponent<Player> ();
    }

    [Test]
    public void InitializeTest ()
    {
        //Arrange

        //Act
        //Try to initialize
        int id = 0;
        int enemyIndex = 0;
        int level = 0;
        Vector3 spawnPoint = new Vector3 (0, 0, 0);
        float maxHp = 10;
        float damagedHp = 1;
        enemy.Initialize (id, enemyIndex, level, spawnPoint, maxHp, damagedHp, null);


        //Assert
        //The enemy is initialized correctly
        Assert.AreEqual (id, enemy.id);
        Assert.AreEqual (enemyIndex, enemy.enemyIndex);
        Assert.AreEqual (level, enemy.level);
        Assert.AreEqual (maxHp, enemy.maxHp);
        Assert.AreEqual (spawnPoint, enemy.spawnPoint);
        Assert.AreEqual (damagedHp, enemy.damagedHp);
    }

    [Test]
    public void EnemyGetHitTest ()
    {

        // Arrange
        float damage = 1.5f;
        float initDamagedHp = 0.5f;
        enemy.damagedHp = initDamagedHp;

        // Act
        // Try to get hit
        enemy.OnHit (damage);

        // Assert
        // The enemy's damaged hp should be the sum of aboved 2
        Assert.AreEqual (damage + initDamagedHp, enemy.damagedHp);
    }

    [Test]
    public void EnemyMoveTest ()
    {

        // Arrange
        Vector3 enemyStartPos = new Vector3 (0, 0, 0);
        Vector3 playerStartPos = new Vector3 (0, 0, 10);

        InitTest ();

        enemyObject.transform.position = enemyStartPos;
        playerObject.transform.position = playerStartPos;

        // make enemy move a bit
        enemy.Move ();

        // Assertion
        // enmey is not at the same position
        Assert.AreNotSame (enemyStartPos, enemy.transform.position);
        // enemy is walking
        Assert.AreEqual (true, enemy.isWalking);

        // then set hate to a player
        enemy.SetHatePlayer (player);
        enemy.Move ();

        // Assertion
        // enemy is running
        Assert.AreEqual (true, enemy.isRunning);

    }

    [Test]
    public void EnemyAttackTest ()
    {
        // Arrange
        Vector3 enemyStartPos = new Vector3 (0, 0, 0);
        Vector3 playerStartPos = new Vector3 (0, 0, 3);

        InitTest ();

        enemyObject.transform.position = enemyStartPos;
        playerObject.transform.position = playerStartPos;

        // let enemy attack
        enemy.Attack ();

        // Assertion
        // enemy is not attacking since not set hate player
        Assert.AreEqual (false, enemy.isAttacking);

        // Set Hate Player
        enemy.SetHatePlayer (player);
        enemy.Attack ();

        // now it should be true

        Assert.AreEqual (true, enemy.isAttacking);
    }

    void InitTest ()
    {
        enemy.spawnPoint = new Vector3 (0, 0, 0);
        enemy.hateRange = 100;
        enemy.attractedActiveRange = 100;
        enemy.normalActiveRange = 80;
        enemy.attackRange = 5;
        enemy.attackSpeed = 1;
        enemy.moveSpeed = 5;
        enemy.rotateSpeed = 90;
        enemy.isWalking = false;
        enemy.isRunning = false;
        enemy.isAttacking = false;
        enemy.SetHatePlayer (null);
    }
}
