![context-js logo](https://i.ibb.co/LJ100w9/image.png)
(image: fake context-menu for Chrome, dark theme)

# About
Context.js is a context-menu library for web and of course written in JavaScript.\
You can try out this [demo](https://heapoverride.sexy/contextjs).

# Installation
### Step 1
Copy **context** folder to your project directory.
### Step 2
Link CSS styles to your HTML code
```html
<link rel="stylesheet" href="./context/skins/chrome-dark.css" type="text/css" />
```
### Step 3
Link Context.js to your HTML code
```html
<script src="./context/context.js"></script>
```

# Example
```js
const clickHandler = e => {
    e.label.innerText = e.label.innerText.split('').reverse().join('');
    e.data.text = e.label.innerText;
    e.handled = true;
}

const contextMenu = new ContextMenu(document.body, [
    {text: 'Back', hotkey: 'Alt+Left arrow', disabled: true, onclick: clickHandler},
    {text: 'Forward', hotkey: 'Alt+Right arrow', disabled: true, onclick: clickHandler},
    {text: 'Reload', hotkey: 'Ctrl+R', onclick: clickHandler},
    null,
    {text: 'Save as...', hotkey: 'Ctrl+S', onclick: clickHandler},
    {text: 'Print...', hotkey: 'Ctrl+P', onclick: clickHandler},
    {text: 'Cast...', onclick: clickHandler},
    {text: 'Translate to English', onclick: clickHandler},
    null,
    {text: 'View page source', hotkey: 'Ctrl+U', onclick: clickHandler},
    {text: 'Inspect', hotkey: 'Ctrl+Shift+I', onclick: clickHandler},
]);

contextMenu.install();
```

# Properties
**text** - Menu item's text\
**hotkey** - Menu item's hotkey text\
**color** - Menu item's custom text color (hexadecimal notation, #RRGGBB)\
**disabled** - A boolean value that when set to true will mark this item disabled (`onclick` is not called if item is disabled)\
**onclick** - A function that is called when user clicks on this item\
**subitems** - Array of menu item's subitems (`onclick` is ignored if item has subitems)\
**submenu** - ContextMenu instance that will be shown here (doesn't work together with `subitems`)

# Public methods
```js
/* Show ContextMenu at specific position */
ContextMenu.show(x, y)
```
```js
/* Hide this ContextMenu DOM element and it's child elements */
ContextMenu.hide()
```
```js
/* Add event listeners for this ContextMenu object */
ContextMenu.install()
```
```js
/* Remove event listeners and DOM elements 
   associated with this ContextMenu object */
ContextMenu.uninstall()
```

# CSS classes
![css](https://i.gyazo.com/176051429fcf2f25b18a66ad2d4e0532.png)

# Default skins
### chrome-dark.css
![chrome-bright](https://i.ibb.co/SxjSWdK/image.png)
### chrome-bright.css
![chrome-bright](https://i.ibb.co/bzB3sPb/image.png)
### hackerman.css
![chrome-bright](https://i.ibb.co/nwtjHqx/image.png)
### kali_dark.css
![kali_dark](https://i.ibb.co/bLDfxcF/image.png)
