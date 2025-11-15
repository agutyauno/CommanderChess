using System.Collections.Generic;
using System.Linq;
using CommanderChess.Domain;
using CommanderChess.GameState;
using CommanderChess.Presentation;
using CommanderChess.Services;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace CommanderChess.UI.Controllers
{
    public class GameHUDController : MonoBehaviour
    {
        // Dependencies
        [Inject] readonly GameStateManager gameStateManager;
        [Inject] readonly TurnManager turnManager;
        [Inject] readonly CarryingSystem carryingSystem;
        [Inject] readonly BoardHighlighter boardHighlighter;
        [Inject] readonly EventBus eventBus;
        [Inject] readonly ZoneProvider zoneProvider;

        UIDocument document;
        VisualElement root;

        // UI Elements
        VisualElement infoPanel;
        Button btnRof;
        Button btnConfirm;
        Button btnCancel;

        // State
        BasePiece carrierPiece;
        BasePiece currentSelectedPiece;
        readonly List<InfoPanelItem> infoPanelItems = new();
        bool isShowingRoF;

        struct InfoPanelItem
        {
            public VisualElement Root;
            public VisualElement Icon;
            public Label NameLabel;
            public BasePiece Piece;
        }

        private void Awake() 
        {
            document = GetComponent<UIDocument>();
            root = document.rootVisualElement;
        }

        private void OnEnable() 
        {
            cacheUIElements();
            SubscribeToEvents();
            SetupButton();
        }

        private void OnDisable() 
        {
            UnsubscribeFromEvents();
        }
        
        #region UI Setup
        void cacheUIElements()
        {
            infoPanel = root.Q<VisualElement>("info-panel");
            btnRof = root.Q<Button>("btn_rof");
            btnConfirm = root.Q<Button>("btn_confirm");
            btnCancel = root.Q<Button>("btn_cancel");

            if (infoPanel == null) Debug.LogError("info-panel not found!");
            if (btnRof == null) Debug.LogError("btn_rof not found!");
            if (btnConfirm == null) Debug.LogError("btn_confirm not found!");
            if (btnCancel == null) Debug.LogError("btn_cancel not found!");
        }

        void SetupButton()
        {
            btnRof.clicked += OnRofButtonClicked;
            btnConfirm.clicked += OnConfirmButtonClicked;
            btnCancel.clicked += OnCancelButtonClicked;
        }
        #endregion

        #region Event Subscripiton
        void SubscribeToEvents()
        {
            eventBus.Subscribe<PieceSelectedEvent>(OnPieceSelected);
            eventBus.Subscribe<PieceDeselectedEvent>(OnPieceDeselected);   
        }

        void UnsubscribeFromEvents()
        {
            eventBus.Unsubscribe<PieceSelectedEvent>(OnPieceSelected);
            eventBus.Unsubscribe<PieceDeselectedEvent>(OnPieceDeselected);   
        }
        #endregion

        #region Event Handlers
        void OnPieceSelected(PieceSelectedEvent evt)
        {
            infoPanel.visible = true;
            carrierPiece = evt.Piece;
            currentSelectedPiece = evt.Piece;
            ReBuildInfoPanel();
            UpdatePieceSelection();
        }

        void OnPieceDeselected(PieceDeselectedEvent evt)
        {
            infoPanelItems.Clear();
            currentSelectedPiece = null;
            infoPanel.visible = false;
        }
        #endregion

        #region Info Panel Management
        void ReBuildInfoPanel()
        {
            ClearInfoPanel();   
            foreach (var piece in carryingSystem.GetAllCarriedPieces(carrierPiece))
            {
                AddInfoPanelItem(piece);
            }
        }

        void AddInfoPanelItem(BasePiece piece)
        {
            var item = new VisualElement();
            item.AddToClassList("info-panel-item");

            var icon = new VisualElement();
            icon.AddToClassList("piece-image");
            var sprite = GetPieceIcon(piece);
            if (sprite != null)
            {
                icon.style.backgroundImage = new StyleBackground(sprite);
            }

            var nameLabel = new Label(GetPieceDisplayName(piece));
            nameLabel.AddToClassList("piece-name");

            item.Add(icon);
            item.Add(nameLabel);

            item.RegisterCallback<ClickEvent>(evt => {
                OnInfoPanelItemClicked(piece);
            });

            infoPanel.Add(item);

            infoPanelItems.Add(new InfoPanelItem
            {
                Root = item,
                Icon = icon,
                NameLabel = nameLabel,
                Piece = piece
            });
        }

        void ClearInfoPanel()
        {
            infoPanelItems.Clear();
            infoPanel.Clear();
        }

        void UpdatePieceSelection()
        {
            foreach (var item in infoPanelItems)
            {
                if (item.Piece == currentSelectedPiece)
                {
                    item.Root.AddToClassList("selected");
                }
                else
                {
                    item.Root.RemoveFromClassList("selected");
                }
            }
        }

        void OnInfoPanelItemClicked(BasePiece piece)
        {
            currentSelectedPiece = piece;
            UpdatePieceSelection();
        }
        #endregion

        #region Button Handlers
        void OnRofButtonClicked()
        {
            if (isShowingRoF)
            {
                HideRof();
                isShowingRoF = false;
            }
            else
            {
                ShowRof();
                isShowingRoF = true;
            }
        }

        void OnConfirmButtonClicked()
        {
            
        }

        void OnCancelButtonClicked()
        {
            
        }

        void ShowRof()
        {
            var rof = zoneProvider.GetROF();
            boardHighlighter.HighlightDangerZones(rof.GetZone().ToList());
        }

        void HideRof()
        {
            boardHighlighter.ClearDangers();
        }
        #endregion

        #region Utility
        Sprite GetPieceIcon(BasePiece piece)
        {
            var spritePath = $"project://database/Assets/Sprites/Pieces/{piece.Type}_{piece.Team}Side.png";
            var sprite = Resources.Load<Sprite>(spritePath);
            if (sprite == null)
            {
                Debug.LogError($"Sprite not found at path: {spritePath}");
            }
            return sprite;
        }

        string GetPieceDisplayName(BasePiece piece)
        {
            return $"{piece.Type}";
        }
        #endregion
    }
}
