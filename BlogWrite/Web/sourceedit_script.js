

function toggleTheme(theme)
{
    if (theme === 'dark')
    {
        //document.body.classList.remove('light-theme');
        document.body.classList.add('dark-theme');
    }
    else if (theme === 'light')
    {
        document.body.classList.remove('dark-theme');
        //document.body.classList.add('light-theme');
    }
}

function focusEditor() {
    document.getElementById('editor').focus();
}



function escapeHtml(htmlStr)
{
    return htmlStr.replace(/&/g, '&amp;')
         .replace(/ /g, '&nbsp;')
         .replace(/</g, '&lt;')
         .replace(/>/g, '&gt;')
         .replace(/""/g, '&quot;')
         .replace(/'/g, '&#39;');
}

function strip_tags(html, ...args)
{
    return html.replace(/<(\/?)(\w+)[^>]*\/?>/g, (_, endMark, tag) => {
        return args.includes(tag) ? '<' + endMark + tag + '>' : '';
    }).replace(/<!--.*?-->/g, '');
}


async function OnPaste(e) {
    e.preventDefault();

    if (e.clipboardData.types.indexOf('text/html') > -1) {
        var text = e.clipboardData.getData('text/html');

        //text = await navigator.clipboard.readText();

        if (text !== '') {
            console.log(text);
            //text=escapeHtml(text);
            //text=text.replace(/\n/g,'<br/>');
            text = strip_tags(text, 'b', 'em', 'strong', 'mark', 'i', 'u', 'p', 'br', 'div', 'span', 'h1', 'h2', 'h3', 'h4', 'ul', 'li', 'ui', 'ol', 'blockquote', 'web-copy-code', 'pre', 'code', 'dd', 'dt', 'dl');
            //console.log(text);

            /*
             * This is OK, but Undo/Redo won't work.
            const range = window.getSelection().getRangeAt(0);
            const documentFragment = range.createContextualFragment(text);
            range.deleteContents();
            range.insertNode(documentFragment);
            range.collapse();
            */

            document.execCommand("insertHTML", false, text);

            //var parentNode = range.commonAncestorContainer;
            //parentNode.innerHTML = newNode.innerHTML + text;

            /*
            var newNode = document.createElement('span');
            newNode.innerHTML = text;

            const range = window.getSelection().getRangeAt(0);
            range.deleteContents();

            //const textNode = document.createTextNode(text);
            range.insertNode(newNode);
            range.selectNodeContents(newNode);
            range.collapse(false);

            const selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(range);
            */

            // Update HTML Source to native-side.
            window.chrome.webview.postMessage(document.getElementById('editor').innerHTML);


            /*
            if (window.getSelection) {
                var newNode = document.createElement('span');
                newNode.innerHTML = text;
                window.getSelection().getRangeAt(0).insertNode(newNode);

            } else {
                document.selection.createRange().pasteHTML(text);
            }
            */

            console.log('html');
        }

    }
    else {
        var text = e.clipboardData
            ? (e.originalEvent || e).clipboardData.getData('text/plain')
            : // For IE
            window.clipboardData
                ? window.clipboardData.getData('text')
                : '';

        if (text !== '') {

            text = escapeHtml(text);
            text = text.replace(/\n/g, '<br/>');

            /*
             * This is OK, but Undo/Redo won't work.
            const range = window.getSelection().getRangeAt(0);
            const documentFragment = range.createContextualFragment(text);
            range.deleteContents();
            range.insertNode(documentFragment);
            range.collapse();
            */


            document.execCommand("insertHTML", false, text);


            /*
            var newNode = document.createElement('span');
            newNode.innerHTML = text;

            const range = document.getSelection().getRangeAt(0);
            range.deleteContents();

            //const textNode = document.createTextNode(text);
            range.insertNode(newNode);
            range.selectNodeContents(newNode);
            range.collapse(false);

            const selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(range);
            */

            // Update HTML Source to native-side.
            window.chrome.webview.postMessage(document.getElementById('editor').innerHTML);

            /*
            if (window.getSelection) {
                var newNode = document.createElement('span');
                newNode.innerHTML = text;
                window.getSelection().getRangeAt(0).insertNode(newNode);

            } else {
                document.selection.createRange().pasteHTML(text);
            }
            */

            console.log('text');
        }
    }


    /*
        // Insert text at the current position of caret
        const range = document.getSelection().getRangeAt(0);
        range.deleteContents();
    
        const textNode = document.createTextNode(text);
        range.insertNode(textNode);
        range.selectNodeContents(textNode);
        range.collapse(false);
    
        const selection = window.getSelection();
        selection.removeAllRanges();
        selection.addRange(range);
    */

}


