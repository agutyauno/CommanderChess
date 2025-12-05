using System.Collections.Generic;
using CommanderChess.Commands;
using CommanderChess.Core;
using CommanderChess.Managers;
using CommanderChess.Pieces;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CommanderChess.UI
{
    /// <summary>
    /// UI for displaying and using Commander abilities
    /// </summary>
    public class CommanderAbilityUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _abilityPanel;
        [SerializeField] private Button[] _abilityButtons;
        [SerializeField] private TextMeshProUGUI[] _abilityTexts;
        [SerializeField] private TextMeshProUGUI _cooldownText;
        [SerializeField] private TextMeshProUGUI _abilityDescriptionText;

        [Header("Colors")]
        [SerializeField] private Color _availableColor = Color.green;
        [SerializeField] private Color _unavailableColor = Color.gray;
        [SerializeField] private Color _cooldownColor = Color.yellow;

        private Commander _currentCommander;
        private CommanderAbility _selectedAbility = CommanderAbility.None;
        private bool _waitingForTarget;

        private readonly CommanderAbility[] _allAbilities = 
        {
            CommanderAbility.Rally,
            CommanderAbility.Charge,
            CommanderAbility.Shield,
            CommanderAbility.Tactics,
            CommanderAbility.Inspire
        };

        private void Start()
        {
            SetupButtons();
            SubscribeToEvents();
            UpdateUI();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SetupButtons()
        {
            if (_abilityButtons == null)
                return;

            for (int i = 0; i < _abilityButtons.Length && i < _allAbilities.Length; i++)
            {
                int index = i; // Capture for lambda
                if (_abilityButtons[i] != null)
                {
                    _abilityButtons[i].onClick.AddListener(() => OnAbilityButtonClicked(index));
                }
            }
        }

        private void SubscribeToEvents()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.OnTurnChanged += OnTurnChanged;
            GameManager.Instance.OnAbilityUsed += OnAbilityUsed;
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.OnTurnChanged -= OnTurnChanged;
            GameManager.Instance.OnAbilityUsed -= OnAbilityUsed;
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnAbilityButtonClicked(int index)
        {
            if (index < 0 || index >= _allAbilities.Length)
                return;

            var ability = _allAbilities[index];
            SelectAbility(ability);
        }

        private void SelectAbility(CommanderAbility ability)
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsCommanderMode)
                return;

            var availableAbilities = GameManager.Instance.GetAvailableAbilities();
            
            if (!availableAbilities.Contains(ability))
            {
                ShowMessage($"{ability} is not available!");
                return;
            }

            // Check if ability needs a target
            if (NeedsTarget(ability))
            {
                _selectedAbility = ability;
                _waitingForTarget = true;
                ShowMessage($"Select a target for {ability}");
            }
            else
            {
                // Execute immediately
                GameManager.Instance.UseAbility(ability, null);
            }

            UpdateUI();
        }

        /// <summary>
        /// Called when a position is clicked while waiting for ability target
        /// </summary>
        public bool TryExecuteSelectedAbility(BoardPosition target)
        {
            if (!_waitingForTarget || _selectedAbility == CommanderAbility.None)
                return false;

            bool success = GameManager.Instance.UseAbility(_selectedAbility, target);

            if (success)
            {
                CancelAbilitySelection();
            }
            else
            {
                ShowMessage("Invalid target!");
            }

            return success;
        }

        public void CancelAbilitySelection()
        {
            _selectedAbility = CommanderAbility.None;
            _waitingForTarget = false;
            UpdateUI();
        }

        private bool NeedsTarget(CommanderAbility ability)
        {
            return ability switch
            {
                CommanderAbility.Charge => true,
                CommanderAbility.Tactics => true,
                CommanderAbility.Inspire => true,
                _ => false
            };
        }

        private void UpdateUI()
        {
            if (GameManager.Instance == null)
            {
                SetPanelVisible(false);
                return;
            }

            if (!GameManager.Instance.IsCommanderMode)
            {
                SetPanelVisible(false);
                return;
            }

            SetPanelVisible(true);

            // Get current player's commander
            _currentCommander = GameManager.Instance.Board?.GetCommander(GameManager.Instance.CurrentPlayer);

            if (_currentCommander == null || _currentCommander.IsCaptured)
            {
                SetPanelVisible(false);
                return;
            }

            var availableAbilities = GameManager.Instance.GetAvailableAbilities();

            // Update buttons
            for (int i = 0; i < _abilityButtons.Length && i < _allAbilities.Length; i++)
            {
                if (_abilityButtons[i] == null)
                    continue;

                var ability = _allAbilities[i];
                bool isAvailable = availableAbilities.Contains(ability);
                
                _abilityButtons[i].interactable = isAvailable;

                // Update button color
                var colors = _abilityButtons[i].colors;
                colors.normalColor = isAvailable ? _availableColor : _unavailableColor;
                _abilityButtons[i].colors = colors;

                // Update text
                if (_abilityTexts != null && i < _abilityTexts.Length && _abilityTexts[i] != null)
                {
                    _abilityTexts[i].text = ability.ToString();
                }
            }

            // Update cooldown display
            if (_cooldownText != null)
            {
                if (_currentCommander.AbilityCooldown > 0)
                {
                    _cooldownText.text = $"Cooldown: {_currentCommander.AbilityCooldown} turns";
                }
                else
                {
                    _cooldownText.text = "";
                }
            }

            // Update description if waiting for target
            if (_abilityDescriptionText != null)
            {
                if (_waitingForTarget)
                {
                    _abilityDescriptionText.text = CommanderAbilityHandler.GetAbilityDescription(_selectedAbility);
                }
                else
                {
                    _abilityDescriptionText.text = "";
                }
            }
        }

        private void SetPanelVisible(bool visible)
        {
            if (_abilityPanel != null)
            {
                _abilityPanel.SetActive(visible);
            }
        }

        private void ShowMessage(string message)
        {
            if (_abilityDescriptionText != null)
            {
                _abilityDescriptionText.text = message;
            }
            Debug.Log($"[Ability] {message}");
        }

        #region Event Handlers

        private void OnTurnChanged(PlayerSide player)
        {
            CancelAbilitySelection();
            UpdateUI();
        }

        private void OnAbilityUsed(Commander commander, CommanderAbility ability)
        {
            ShowMessage($"{commander.Side} Commander used {ability}!");
            UpdateUI();
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.NotStarted || 
                state == GameState.WhiteWins || 
                state == GameState.BlackWins ||
                state == GameState.Stalemate ||
                state == GameState.Draw)
            {
                SetPanelVisible(false);
            }
            else
            {
                UpdateUI();
            }
        }

        #endregion

        /// <summary>
        /// Get whether we're waiting for an ability target
        /// </summary>
        public bool IsWaitingForTarget => _waitingForTarget;

        /// <summary>
        /// Get the currently selected ability
        /// </summary>
        public CommanderAbility SelectedAbility => _selectedAbility;
    }
}
