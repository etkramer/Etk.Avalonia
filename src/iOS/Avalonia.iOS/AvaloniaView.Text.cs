using Foundation;
using ObjCRuntime;
using Avalonia.Input.TextInput;
using Avalonia.Input;
using Avalonia.Input.Raw;
using UIKit;

namespace Avalonia.iOS;

#nullable enable

[Adopts("UITextInputTraits")]
[Adopts("UIKeyInput")]
public partial class AvaloniaView : ITextInputMethodImpl
{
    private ITextInputMethodClient? _currentClient;

    public override bool CanResignFirstResponder => true;
    public override bool CanBecomeFirstResponder => true;

    [Export("hasText")]
    public bool HasText
    {
        get
        {
            if (_currentClient is { } && _currentClient.SupportsSurroundingText &&
                _currentClient.SurroundingText.Text.Length > 0)
            {
                return true;
            }

            return false;
        }
    }

    [Export("keyboardType")] public UIKeyboardType KeyboardType { get; private set; } = UIKeyboardType.Default;

    [Export("isSecureTextEntry")] public bool IsSecureEntry { get; private set; }

    [Export("insertText:")]
    public void InsertText(string text)
    {
        if (KeyboardDevice.Instance is { })
        {
            _topLevelImpl.Input?.Invoke(new RawTextInputEventArgs(KeyboardDevice.Instance,
                0, InputRoot, text));
        }
    }

    [Export("deleteBackward")]
    public void DeleteBackward()
    {
        if (KeyboardDevice.Instance is { })
        {
            // TODO: pass this through IME infrastructure instead of emulating a backspace press
            _topLevelImpl.Input?.Invoke(new RawKeyEventArgs(KeyboardDevice.Instance,
                0, InputRoot, RawKeyEventType.KeyDown, Key.Back, RawInputModifiers.None));

            _topLevelImpl.Input?.Invoke(new RawKeyEventArgs(KeyboardDevice.Instance,
                0, InputRoot, RawKeyEventType.KeyUp, Key.Back, RawInputModifiers.None));
        }
    }

    void ITextInputMethodImpl.SetClient(ITextInputMethodClient? client)
    {
        _currentClient = client;

        if (client is { })
        {
            BecomeFirstResponder();
        }
        else
        {
            ResignFirstResponder();
        }
    }

    void ITextInputMethodImpl.SetCursorRect(Rect rect)
    {

    }

    void ITextInputMethodImpl.SetOptions(TextInputOptions options)
    {
        switch (options.ContentType)
        {
            case TextInputContentType.Email:
                KeyboardType = UIKeyboardType.EmailAddress;
                break;

            case TextInputContentType.Number:
                KeyboardType = UIKeyboardType.NumberPad;
                break;

            case TextInputContentType.Password:
                IsSecureEntry = true;
                break;

            case TextInputContentType.Phone:
                KeyboardType = UIKeyboardType.PhonePad;
                break;

            case TextInputContentType.Url:
                KeyboardType = UIKeyboardType.Url;
                break;
            
            case TextInputContentType.Normal:
                KeyboardType = UIKeyboardType.Default;
                break;
        }
    }

    void ITextInputMethodImpl.Reset()
    {
        ResignFirstResponder();
    }
}
