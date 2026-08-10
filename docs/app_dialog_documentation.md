# AppDialog Documentation

`AppDialog` is a globally accessible, Promise-based modal system designed to completely replace native browser dialogs (`confirm()`, `alert()`, `prompt()`). It provides a premium, responsive, and glassmorphic UI that matches the ClexAn Foods brand aesthetic.

## 🌟 Features
- **Asynchronous API**: Returns Promises, allowing you to use modern `async/await` syntax.
- **Contextual Themes**: Supports `info`, `success`, `warning`, and `danger` themes with matching SVG icons.
- **Form Interceptor**: Includes a global `data-confirm` interceptor that seamlessly handles async confirmations for native HTML buttons and links without writing custom JS per form.
- **Zero-Dependency**: Built entirely with Vanilla JS and CSS in `site.js` and `components.css`.

---

## 🛠️ Global Interceptor (Recommended)

The easiest way to use `AppDialog` is via HTML attributes. The global interceptor listens for clicks on any element with the `data-confirm` attribute, halts the action, shows the dialog, and programmatically resumes the action (form submission or navigation) if the user confirms.

### Basic Form Submission
```html
<form method="post" asp-page-handler="Delete">
    <input type="hidden" name="id" value="123" />
    <!-- Instead of onclick="return confirm('...')" -->
    <button type="submit" data-confirm="Are you sure you want to delete this?">Delete</button>
</form>
```

### Advanced Attributes
You can customize the dialog directly from HTML:
- `data-confirm`: The message body (Required to trigger the interceptor).
- `data-confirm-title`: The modal title (Defaults to "Confirm").
- `data-confirm-type`: The theme/icon type (`info`, `success`, `warning`, `danger`).

```html
<button type="submit" 
        class="danger"
        data-confirm="Permanently delete this customer profile?" 
        data-confirm-title="Delete Customer" 
        data-confirm-type="danger">
    Delete Profile
</button>
```

---

## 💻 JavaScript API

If you need programmatic control inside your custom scripts, you can call the `AppDialog` API directly.

### 1. Confirm (`AppDialog.confirm`)
Returns a Promise that resolves to `true` (Confirm) or `false` (Cancel).

```javascript
document.getElementById('myButton').addEventListener('click', async () => {
    const isConfirmed = await window.AppDialog.confirm({
        title: 'Archive Record',
        message: 'Are you sure you want to archive this record?',
        type: 'warning', // info, success, warning, danger
        confirmText: 'Yes, Archive',
        cancelText: 'Cancel'
    });

    if (isConfirmed) {
        // Proceed with action
    }
});
```

### 2. Alert (`AppDialog.alert`)
Returns a Promise that resolves to `true` when the user clicks OK. Hides the cancel button.

```javascript
await window.AppDialog.alert({
    title: 'Validation Error',
    message: 'Please enter a valid non-zero points value.',
    type: 'danger'
});
```
*Note: Shorthand is also supported: `await window.AppDialog.alert('Error message here!');`*

### 3. Prompt (`AppDialog.prompt`)
Returns a Promise that resolves to the entered string, or `null` if cancelled.

```javascript
const reason = await window.AppDialog.prompt({
    title: 'Refund Reason',
    message: 'Please provide a reason for this refund:',
    defaultValue: 'Customer requested' // Optional
});

if (reason !== null) {
    console.log("Refund reason:", reason);
}
```

---

## 🧩 Architecture

The `AppDialog` system consists of three parts:
1. **HTML Skeleton**: Stored globally in `_AppLayout.cshtml` and `_Layout.cshtml` (`#appDialogOverlay`).
2. **CSS Styling**: Stored in `components.css` (`.dialog-overlay`, `.dialog-box`, etc.).
3. **JS Logic**: Stored at the bottom of `site.js` inside an IIFE. It fetches DOM elements dynamically on invocation to prevent load-order race conditions.
