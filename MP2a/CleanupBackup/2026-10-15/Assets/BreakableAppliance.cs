using TMPro;
using UnityEngine;

public class BreakableAppliance : MonoBehaviour
{
    public int shellIntegrity = 3;
    public int dropCount = 3;
    public ScrapPickup scrapPrefab;
    public PlayerResources resources;
    public Renderer shell;
    public TMP_Text label;
    private bool broken;
    private float showTime;
    private Camera viewCamera;

    public void ShowHint(DesktopBuildInput input)
    {
        viewCamera = input.viewCamera;
        if (!input.shatterTutorialShown)
        {
            input.shatterTutorialShown = true;
            showTime = 7f;
        }
        else showTime = Mathf.Max(showTime, 0.15f);
        RefreshLabel();
    }

    private void Update()
    {
        if (label == null) return;
        showTime -= Time.unscaledDeltaTime;
        label.gameObject.SetActive(showTime > 0f);
        if (viewCamera != null) label.transform.rotation = viewCamera.transform.rotation;
    }

    private void RefreshLabel()
    {
        if (label == null) return;
        label.text = "BROKEN TOASTER\nSHELL " + shellIntegrity + "\nLeft click: dismantle\nCollect the loose scrap afterward";
        label.gameObject.SetActive(true);
    }

    public string Hit()
    {
        if (broken) return "Already dismantled.";
        if (scrapPrefab == null || resources == null) return "Dismantle references missing.";
        shellIntegrity = Mathf.Max(0, shellIntegrity - 1);
        RefreshLabel();
        if (shell != null)
        {
            MaterialPropertyBlock colors = new MaterialPropertyBlock();
            Color color = shellIntegrity > 1 ? Color.yellow : Color.red;
            colors.SetColor("_BaseColor", color);
            colors.SetColor("_Color", color);
            shell.SetPropertyBlock(colors);
        }
        if (shellIntegrity > 0) return "Shell integrity: " + shellIntegrity + ". Keep dismantling.";

        broken = true;
        for (int i = 0; i < dropCount; i++)
        {
            float angle = i * Mathf.PI * 2f / dropCount;
            Vector3 outward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            ScrapPickup scrap = Instantiate(scrapPrefab, transform.position + Vector3.up * 0.5f + outward * 0.65f, Quaternion.identity);
            scrap.resources = resources;
            Rigidbody body = scrap.GetComponent<Rigidbody>();
            if (body != null) body.AddForce(outward * 0.5f + Vector3.up, ForceMode.Impulse);
        }
        gameObject.SetActive(false);
        Destroy(gameObject);
        return "Dismantled! Collect " + dropCount + " loose scrap pieces.";
    }
}
