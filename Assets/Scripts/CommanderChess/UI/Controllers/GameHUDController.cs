using System.Collections.Generic;
using System.Linq;
using CommanderChess.Domain;
using CommanderChess.GameState;
using CommanderChess.Presentation;
using CommanderChess.Services;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

            infoPanel.visible = false;
        }

        void SetupButton()
        {
            btnRof.clicked += OnRofButtonClicked;
            btnConfirm.clicked += OnConfirmButtonClicked;
            btnCancel.clicked += OnCancelButtonClicked;
            
            // Initially hide confirm/cancel buttons
            btnConfirm.visible = false;
            btnCancel.visible = false;
        }
        #endregion

        #region Event Subscripiton
        void SubscribeToEvents()
        {
            eventBus.Subscribe<PieceSelectedEvent>(OnPieceSelected);
            eventBus.Subscribe<PieceDeselectedEvent>(OnPieceDeselected);
            eventBus.Subscribe<TurnEndConditionReachedEvent>(OnTurnEndConditionReached);
            eventBus.Subscribe<TurnDetachOccurredEvent>(OnTurnDetachOccurred);
            eventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
            eventBus.Subscribe<TurnEndCancelledEvent>(OnTurnEndCancelled);
        }

        void UnsubscribeFromEvents()
        {
            eventBus.Unsubscribe<PieceSelectedEvent>(OnPieceSelected);
            eventBus.Unsubscribe<PieceDeselectedEvent>(OnPieceDeselected);
            eventBus.Unsubscribe<TurnEndConditionReachedEvent>(OnTurnEndConditionReached);
            eventBus.Unsubscribe<TurnDetachOccurredEvent>(OnTurnDetachOccurred);
            eventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
            eventBus.Unsubscribe<TurnEndCancelledEvent>(OnTurnEndCancelled);
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
            infoPanel.Clear();
            currentSelectedPiece = null;
            infoPanel.visible = false;
        }

        void OnTurnEndConditionReached(TurnEndConditionReachedEvent evt)
        {
            // Show confirm/cancel buttons
            btnConfirm.visible = true;
            btnCancel.visible = true;
            Debug.Log($"[GameHUD] Turn end condition reached: {evt.CommandDescription}");
        }

        void OnTurnDetachOccurred(TurnDetachOccurredEvent evt)
        {
            // Detach happened - carrier can still act, don't show confirm yet
            Debug.Log($"[GameHUD] Detach occurred - {evt.AllowedPiece.Type} can continue acting");
        }

        void OnTurnEnded(TurnEndedEvent evt)
        {
            // Hide confirm/cancel buttons
            btnConfirm.visible = false;
            btnCancel.visible = false;
            Debug.Log($"[GameHUD] Turn ended for {evt.Team}");
        }

        void OnTurnEndCancelled(TurnEndCancelledEvent evt)
        {
            // Hide confirm/cancel buttons
            btnConfirm.visible = false;
            btnCancel.visible = false;
            Debug.Log($"[GameHUD] Turn cancelled for {evt.Team}");
        }
        #endregion

        #region Info Panel Management
        void ReBuildInfoPanel()
        {
            ClearInfoPanel();
            AddInfoPanelItem(carrierPiece);   
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
            // If clicking the same piece, deselect it
            if (currentSelectedPiece == piece)
            {
                Debug.Log($"Deselecting {piece.Type}");
                
                // If piece is carried, go back to selecting carrier
                if (carryingSystem.IsCarried(piece))
                {
                    var carrier = carryingSystem.GetCarrier(piece);
                    if (carrier != null)
                    {
                        currentSelectedPiece = carrier;
                        UpdatePieceSelection();
                        
                        // Select carrier in game state
                        gameStateManager.SelectPiece(carrier);
                    }
                }
                else
                {
                    // Deselect and go back to idle
                    currentSelectedPiece = null;
                    UpdatePieceSelection();
                    gameStateManager.CancelCurrentAction();
                }
                return;
            }

            // Update UI selection
            currentSelectedPiece = piece;
            UpdatePieceSelection();

            // Check if piece is carried (need to detach)
            if (carryingSystem.IsCarried(piece))
            {
                Debug.Log($"Selected carried piece {piece.Type} - entering detach mode");
                gameStateManager.SelectPieceForDetach(piece);
            }
            else
            {
                // Piece is carrier or standalone - select normally
                Debug.Log($"Selected piece {piece.Type}");
                gameStateManager.SelectPiece(piece);
            }
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
            Debug.Log("[GameHUD] Confirm button clicked - ending turn");
            turnManager.ConfirmEndTurn();
        }

        void OnCancelButtonClicked()
        {
            Debug.Log("[GameHUD] Cancel button clicked - cancelling turn");
            turnManager.CancelEndTurn();
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
            #if UNITY_EDITOR
            // Use AssetDatabase in Editor
            var spritePath = $"Assets/Sprites/Pieces/{piece.Type}_{piece.Team}Side.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            
            if (sprite == null)
            {
                Debug.LogError($"Sprite not found at path: {spritePath}");
                Debug.LogError($"Make sure the file exists and naming matches: {piece.Type}_{piece.Team}Side.png");
            }
            
            return sprite;
            #else
            // Use Resources.Load in Build
            // Sprites must be in Assets/Resources/Pieces/ folder for builds
            var spritePath = $"Pieces/{piece.Type}_{piece.Team}Side";
            var sprite = Resources.Load<Sprite>(spritePath);
            
            if (sprite == null)
            {
                Debug.LogError($"Sprite not found at Resources path: {spritePath}");
                Debug.LogError($"Make sure sprites are in Assets/Resources/Pieces/ folder for builds");
            }
            
            return sprite;
            #endif
        }

        string GetPieceDisplayName(BasePiece piece)
        {
            return $"{piece.Type}";
        }
        #endregion
    }
}
