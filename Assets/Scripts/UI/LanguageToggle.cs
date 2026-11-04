using SortingVR.Core;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SortingVR.UI
{
    /// <summary>
    /// Two tabs (Python / C) on the code board, pressed with the controller ray.
    /// Switches the code board and the variables board. Event driven, no per-frame work.
    /// </summary>
    public sealed class LanguageToggle : MonoBehaviour
    {
        [SerializeField] private CodeBoardView codeView;
        [SerializeField] private VariablesBoardView variablesView;
        [SerializeField] private CodeLanguage startLanguage = CodeLanguage.Python;

        [Header("Python tab")]
        [SerializeField] private XRSimpleInteractable pythonTab;
        [SerializeField] private Renderer pythonBack;
        [SerializeField] private TMP_Text pythonLabel;

        [Header("C tab")]
        [SerializeField] private XRSimpleInteractable cTab;
        [SerializeField] private Renderer cBack;
        [SerializeField] private TMP_Text cLabel;

        [Header("Look")]
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material inactiveMaterial;
        [SerializeField] private Color activeTextColor = new Color(0.11f, 0.129f, 0.149f);   // #1C2126
        [SerializeField] private Color inactiveTextColor = new Color(0.788f, 0.827f, 0.859f); // #C9D3DB
        [SerializeField] private Color hoverTextColor = new Color(1f, 0.851f, 0.239f);       // #FFD93D

        private CodeLanguage language;
        private bool pythonHovered;
        private bool cHovered;

        private void OnEnable()
        {
            pythonTab.selectEntered.AddListener(OnPythonSelected);
            cTab.selectEntered.AddListener(OnCSelected);
            pythonTab.hoverEntered.AddListener(OnPythonHoverEntered);
            pythonTab.hoverExited.AddListener(OnPythonHoverExited);
            cTab.hoverEntered.AddListener(OnCHoverEntered);
            cTab.hoverExited.AddListener(OnCHoverExited);
        }

        private void OnDisable()
        {
            pythonTab.selectEntered.RemoveListener(OnPythonSelected);
            cTab.selectEntered.RemoveListener(OnCSelected);
            pythonTab.hoverEntered.RemoveListener(OnPythonHoverEntered);
            pythonTab.hoverExited.RemoveListener(OnPythonHoverExited);
            cTab.hoverEntered.RemoveListener(OnCHoverEntered);
            cTab.hoverExited.RemoveListener(OnCHoverExited);
        }

        private void Start()
        {
            SetLanguage(startLanguage);
        }

        public void SetLanguage(CodeLanguage value)
        {
            language = value;
            codeView.SetLanguage(value);
            if (variablesView != null)
            {
                variablesView.SetLanguage(value);
            }

            RefreshTabs();
        }

        private void OnPythonSelected(SelectEnterEventArgs args) => SetLanguage(CodeLanguage.Python);

        private void OnCSelected(SelectEnterEventArgs args) => SetLanguage(CodeLanguage.C);

        private void OnPythonHoverEntered(HoverEnterEventArgs args) { pythonHovered = true; RefreshTabs(); }

        private void OnPythonHoverExited(HoverExitEventArgs args) { pythonHovered = pythonTab.isHovered; RefreshTabs(); }

        private void OnCHoverEntered(HoverEnterEventArgs args) { cHovered = true; RefreshTabs(); }

        private void OnCHoverExited(HoverExitEventArgs args) { cHovered = cTab.isHovered; RefreshTabs(); }

        private void RefreshTabs()
        {
            Style(language == CodeLanguage.Python, pythonHovered, pythonBack, pythonLabel);
            Style(language == CodeLanguage.C, cHovered, cBack, cLabel);
        }

        private void Style(bool active, bool hovered, Renderer back, TMP_Text label)
        {
            back.sharedMaterial = active ? activeMaterial : inactiveMaterial;
            label.color = active ? activeTextColor : hovered ? hoverTextColor : inactiveTextColor;
        }
    }
}
