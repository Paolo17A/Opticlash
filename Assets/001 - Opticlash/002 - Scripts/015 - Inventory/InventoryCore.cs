using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryCore : MonoBehaviour
{
    [field: SerializeField] private PlayerData PlayerData { get; set; }
    //==========================================================================================
    [field: Header("INVENTORY")]
    [field: SerializeField] private Sprite BreakRemoveSprite { get; set; }
    [field: SerializeField] private Sprite BurnRemoveSprite { get; set; }
    [field: SerializeField] private Sprite ConfuseRemoveSprite { get; set; }
    [field: SerializeField] private Sprite FreezeRemoveSprite { get; set; }
    [field: SerializeField] private Sprite SmallHealChargeSprite { get; set; }
    [field: SerializeField] private Sprite MediumHealChargeSprite { get; set; }
    [field: SerializeField] private Sprite LargeHealChargeSprite { get; set; }
    [field: SerializeField] private Sprite ParalyzeRemoveSprite { get; set; }
    [field: SerializeField] private Sprite WeakRemoveSprite { get; set; }
    [field: SerializeField] private Image DisplayedInventoryImage { get; set; }
    [field: SerializeField] private TextMeshProUGUI InventoryItemNameTMP { get; set; }
    [field: SerializeField] private GameObject RectanglePanelInventory { get; set; }
    [field: SerializeField] private TextMeshProUGUI InventoryItemDescriptionTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI InventoryItemCountTMP { get; set; }


    //==========================================================================================

    #region INVENTORY
    public void InitializeInventory()
    {
        DisplayedInventoryImage.gameObject.SetActive(false);
        InventoryItemNameTMP.gameObject.SetActive(false);
        RectanglePanelInventory.SetActive(false);
        SelectBreakRemove();
    }

    public void SelectBreakRemove()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = BreakRemoveSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "BREAK REMOVAL";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to remove negative buff BREAK";
        InventoryItemCountTMP.text = PlayerData.BreakRemovalCharges.ToString();
    }

    public void SelectBurnRemove()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = BurnRemoveSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "BURN REMOVAL";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to remove negative buff BURN";
        InventoryItemCountTMP.text = PlayerData.BurnRemovalCharges.ToString();
    }

    public void SelectConfuseRemove()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = ConfuseRemoveSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "CONFUSE REMOVAL";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to remove negative buff CONFUSE";
        InventoryItemCountTMP.text = PlayerData.ConfuseRemovalCharges.ToString();
    }

    public void SelectFreezeRemove()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = FreezeRemoveSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "FREEZE REMOVAL";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to remove negative buff FREEZE";
        InventoryItemCountTMP.text = PlayerData.FreezeRemovalCharges.ToString();
    }

    public void SelectSmallHeal()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = SmallHealChargeSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "SMALL HEAL CHARGE";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to recover 10 Health Points";
        InventoryItemCountTMP.text = PlayerData.SmallHealCharges.ToString();
    }

    public void SelectMediumHeal()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = SmallHealChargeSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "MEDIUM HEAL CHARGE";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to recover 15 Health Points";
        InventoryItemCountTMP.text = PlayerData.MediumHealCharges.ToString();
    }

    public void SelectLargeHeal()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = SmallHealChargeSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "LARGE HEAL CHARGE";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to recover 25 Health Points";
        InventoryItemCountTMP.text = PlayerData.LargeHealCharges.ToString();
    }

    public void SelectParalyzeRemove()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = ParalyzeRemoveSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "PARALYZE REMOVAL";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to remove negative buff PARALYZE";
        InventoryItemCountTMP.text = PlayerData.ParalyzeRemovalCharges.ToString();
    }

    public void SelectWeakRemove()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = WeakRemoveSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "WEAK REMOVAL";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to remove negative buff WEAK";
        InventoryItemCountTMP.text = PlayerData.WeakRemovalCharges.ToString();
    }
    #endregion
}
