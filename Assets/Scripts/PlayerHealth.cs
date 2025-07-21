using System;
using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth = 0;

    public HeathUI healthUI;
    public Tilemap thornTilemap;

    private SpriteRenderer spriteRenderer;

    public static event Action OnPlayerDied;

    void Start()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if(enemy)
        {
            TakeDamage(enemy.damage);
        }

        //if (thornTilemap != null)
        //{
        //    Vector3 hitPosition = collision.ClosestPoint(transform.position); // Vị trí gần nhất va chạm
        //    Vector3Int cellPosition = thornTilemap.WorldToCell(hitPosition);
        //    TileBase tile = thornTilemap.GetTile(cellPosition);

        //    if (tile != null) // Nếu có tile tại vị trí va chạm, coi là thorn
        //    {
        //        TakeDamage(1); // Mất 1 máu khi chạm thorn
        //    }
        //}

        // Kiểm tra va chạm với Thorn Tilemap
        if (thornTilemap != null)
        {
            Vector3 hitPosition = collision.ClosestPoint(transform.position); // Vị trí gần nhất va chạm
            Vector3Int cellPosition = thornTilemap.WorldToCell(hitPosition);
            TileBase tile = thornTilemap.GetTile(cellPosition);

            if (tile != null) // Nếu có tile tại vị trí va chạm, coi là thorn
            {
                Debug.Log("Thorn Tag Void?: " + thornTilemap.gameObject.CompareTag("Void"));
                // Kiểm tra tag của Tilemap
                if (thornTilemap.gameObject.CompareTag("Void"))
                {
                    TakeDamage(3); // Mất 3 máu nếu tag là "Void"
                    Debug.Log("Hit Thorn Tilemap with tag 'Void', taking 3 damage!");
                }
                else
                {
                    TakeDamage(1); // Mất 1 máu nếu tag không phải "Void"
                    Debug.Log("Hit Thorn Tilemap, taking 1 damage!");
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthUI.UpdateHeart(currentHealth);

        //Flash Red
        StartCoroutine(FlashRed());

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        // Gọi event để thông báo player đã chết
        OnPlayerDied?.Invoke();
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        if (healthUI != null)
        {
            healthUI.UpdateHeart(currentHealth);
        }
        // Đảm bảo player được kích hoạt lại
        gameObject.SetActive(true);
    }
}
