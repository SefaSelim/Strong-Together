using System.Collections;
using UnityEngine;

/// <summary>
/// Bir objeyi Inspector’dan verilen iki nokta (A/B) arasýnda
/// sabit hýzla ileri-geri (ping-pong) hareket ettirir.
/// </summary>
public class BackAndForthMover : MonoBehaviour
{
    [Header("Hedef Noktalar")]
    [Tooltip("Baþlangýç noktasý (A)")]
    [SerializeField] private Transform pointA;
    [Tooltip("Bitiþ noktasý (B)")]
    [SerializeField] private Transform pointB;

    [Header("Hareket Ayarlarý")]
    [Tooltip("m/s cinsinden hýz")]
    [Min(0.01f)]
    [SerializeField] private float speed = 3f;

    [Tooltip("A ve B uçlarýnda beklenecek süre (saniye)")]
    [Min(0f)]
    [SerializeField] private float waitAtEnds = 0f;

    [Tooltip("True ise A konumundan baþlar; false ise B konumundan baþlar")]
    [SerializeField] private bool startAtA = true;

    [Tooltip("Baþlangýçta objeyi seçilen baþlangýç noktasýna yapýþtýr (snap)")]
    [SerializeField] private bool snapToStart = true;

    private Transform _currentTarget;
    private Coroutine _loop;

    private void OnEnable()
    {
        if (!ValidatePoints()) return;
        _loop = StartCoroutine(MoveLoop());
    }

    private void OnDisable()
    {
        if (_loop != null) StopCoroutine(_loop);
    }

    /// <summary>Point A/B atanmýþ mý kontrol eder.</summary>
    private bool ValidatePoints()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("[BackAndForthMover] pointA ve pointB atanmalý!", this);
            enabled = false;
            return false;
        }
        return true;
    }

    /// <summary>Ýleri-geri hareket döngüsü.</summary>
    private IEnumerator MoveLoop()
    {
        if (snapToStart)
            transform.position = (startAtA ? pointA.position : pointB.position);

        _currentTarget = startAtA ? pointB : pointA;

        while (true)
        {
            // Hedefe sabit hýzla ilerle
            while ((transform.position - _currentTarget.position).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    _currentTarget.position,
                    speed * Time.deltaTime
                );
                yield return null;
            }

            // Tam hizalama (yýðýlmayý önler)
            transform.position = _currentTarget.position;

            // Uçta bekle (varsa)
            if (waitAtEnds > 0f)
                yield return new WaitForSeconds(waitAtEnds);

            // Hedefi deðiþtir
            _currentTarget = (_currentTarget == pointA) ? pointB : pointA;
        }
    }

    // Editörde güzergâhý görmek için
    private void OnDrawGizmos()
    {
        if (pointA && pointB)
        {
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.12f);
            Gizmos.DrawWireSphere(pointB.position, 0.12f);
        }
    }
}
