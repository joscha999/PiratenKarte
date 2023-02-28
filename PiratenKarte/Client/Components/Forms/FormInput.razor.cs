using Microsoft.AspNetCore.Components;
using PiratenKarte.Shared.Validation;

namespace PiratenKarte.Client.Components.Forms;

public partial class FormInput<T> {
    [Parameter]
    public string? Title { get; set; }
    [Parameter]
    public FormInputType Type { get; set; }

    private T? _internalValue;
    private T? InternalValue {
        get => _internalValue;
        set {
            if (value == null)
                return;
            if (_internalValue!.Equals(value))
                return;

            _internalValue = value;
            Value = value;
            ValueChanged.InvokeAsync(Value);
        }
    }

    [Parameter]
    public required T Value { get; set; }
    [Parameter]
    public EventCallback<T> ValueChanged { get; set; }

    [Parameter]
    public bool Disabled { get; set; }
    [Parameter]
    public EventCallback SaveClicked { get; set; }

    [Parameter]
    public ErrorBag? ErrorBag { get; set; }
    [Parameter]
    public string? ErrorKey { get; set; }

    [Parameter]
    public string? EditPermission { get; set; }
    [Parameter]
    public bool ShowEditButton { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string Permission => EditPermission ?? "*";

    protected override void OnParametersSet() {
        _internalValue = Value;
        base.OnParametersSet();
    }
}

public enum FormInputType {
    Text,
    TextArea,
    Password,
    Number
}