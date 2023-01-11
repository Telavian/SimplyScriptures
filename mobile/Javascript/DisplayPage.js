var isInitialized = false;

function checkDocumentLoaded() {
    return document.readyState !== 'loading';
}

function checkInitializationComplete() {
    return isInitialized;
}

function initializePageFrame(isInverted) {
    console.log("Initializing page");
    handleFrameZoom();
    addCSSItemsStyle(isInverted);
    addVerseFinder();

    if (isInverted) {
        console.log("Setting dark mode");
        setDarkMode();
    }
    else {
        console.log("Setting light mode");
        setLightMode();
    }

    isInitialized = true;
}

function setDarkMode() {
    var frameWindow = window; /*document.getElementById("pageFrame").contentWindow; */
    var frameWindowDocument = frameWindow.document;
    var container = frameWindowDocument.getElementById("page-container");

    if (!container) {
        console.log("Can't find page container");
        return;
    }

    /* background-color: white; -webkit-filter: ; */
    container.parentElement.style["background-color"] = "#E1E1E1"; /* "#1E1E1E" */
    container.parentElement.style["-webkit-filter"] = "invert()";
}

function setLightMode() {
    var frameWindow = window; /*document.getElementById("pageFrame").contentWindow; */
    var frameWindowDocument = frameWindow.document;
    var container = frameWindowDocument.getElementById("page-container");

    if (!container) {
        console.log("Can't find page container");
        return;
    }

    container.parentElement.style["background-color"] = "white";
    container.parentElement.style["-webkit-filter"] = "";
}

function addCSSItemsStyle(isInverted) {
    console.log("Adding items style");

    var frameWindow = window; /*document.getElementById("pageFrame").contentWindow; */
    var frameWindowDocument = frameWindow.document;
    var styleNode = frameWindowDocument.getElementById('umStyleElement');
    if (styleNode) {
        styleNode.remove();
    }

    var color = isInverted
        ? "#A78960"
        : "#A8C6EF";

    var style = `.ss-search-item { background: ${color} !important; } `;

    if (isInverted) {
        style += "::selection { background: #FF6C6C !important; } ";
    }

    var element = frameWindowDocument.createElement("style");
    element.setAttribute("id", "umStyleElement");
    element.innerText = style;
    frameWindowDocument.head.appendChild(element);
}

function addVerseFinder() {
    console.log("Adding verse finder");

    try {
        var frameWindow = window; /*document.getElementById("pageFrame").contentWindow; */
        var frameWindowDocument = frameWindow.document;
        var container = frameWindowDocument.getElementById("page-container");

        if (frameWindowDocument.getElementById("snackbar") != null) {
            return;
        }

        console.log("Adding snackbar element");
        var snackbar = frameWindowDocument.createElement("div");
        snackbar.id = "snackbar";
        container.appendChild(snackbar);

        console.log("Connecting verse handler");
        container.addEventListener('click', (event) => {
            var [book1, distance1] = findClosest(event.target, "book");
            var [book2, distance2] = findClosest(event.target, "sectiontitle");            
            var [book3, distance3] = findClosest(event.target, "sectionnumber");

            var book = null;
            var isDC = false;
            var isTitle = false;

            if (distance1 < distance2 && distance1 < distance3 && getNodeText(book1).length > 0) {
                book = book1;
            }

            if (distance2 <= distance1 && distance2 <= distance3 && getNodeText(book2).length > 0) {
                book = book2;
                isDC = true;
                isTitle = true;
            }

            if (distance3 <= distance1 && distance3 <= distance2 && getNodeText(book3).length > 0) {
                book = book3;
                isDC = true;
                isTitle = false;
            }

            var fullName = "";

            if (isDC) {                
                var [verse, verseDistance] = findClosest(event.target, "verse");

                if (isTitle) {
                    fullName = `${toProperCase(getNodeText(book))}:${getNodeText(verse)}`;                    
                }
                else {
                    fullName = `Section ${toProperCase(getNodeText(book))}:${getNodeText(verse)}`;
                }
            }
            else {
                var [chapter, chapterDistance] = findClosest(event.target, "chapter");
                var [verse, verseDistance] = findClosest(event.target, "verse");

                fullName = `${toProperCase(getNodeText(book))} ${getNodeText(chapter)}:${getNodeText(verse)}`;
            }

            fullName = fullName.replace(/:+$/, '').trimEnd();

            if (fullName.length == 0) {
                return;
            }            

            var selection = frameWindow.getSelection();

            if (selection != null && selection.type == "Range") {
                return;
            }

            var snackbar = frameWindowDocument.getElementById("snackbar");
            snackbar.innerText = fullName;
            snackbar.className = "show";

            setTimeout(function () { snackbar.className = snackbar.className.replace("show", ""); }, 3000);
        });
    }
    catch (ex) {
        console.log(ex.message);
    }
}

function findClosest(element, name) {
    var distance = 0;

    while (true) {
        if (element == null) {
            break;
        }

        if (element.previousElementSibling != null) {
            element = element.previousElementSibling;
            distance++;

            var isMatch = element.nodeName.toLowerCase() == name.toLowerCase();

            if (isMatch) {
                return [element, distance];
            }

            var foundNode = element.querySelector(name);

            if (foundNode != null) {
                return [foundNode, distance];
            }

            continue;
        }

        distance++;
        element = element.parentElement;
    }

    return [null, distance];
}

function getNodeText(node) {
    if (node == null) {
        return "";
    }

    if (node.innerText == null) {
        return "";
    }

    return node.innerText;
}

function toProperCase(text) {
    let upper = true;
    let newStr = "";

    for (let i = 0, l = text.length; i < l; i++) {
        if (text[i] == " ") {
            upper = true;
            newStr += text[i];
            continue;
        }

        newStr += upper ? text[i].toUpperCase() : text[i].toLowerCase();
        upper = false;
    }

    return newStr;
}

function setCurrentFrameScrollLocation(location) {
    var frameWindow = window; /*document.getElementById("pageFrame").contentWindow; */
    var frameWindowDocument = frameWindow.document;
    var container = frameWindowDocument.getElementById("page-container");

    container.scrollTop = location;
	return true;
}

function getCurrentFrameScrollLocation() {
    var frameWindow = window; /* document.getElementById("pageFrame").contentWindow; */
    var frameWindowDocument = frameWindow.document;
    var container = frameWindowDocument.getElementById("page-container");

    /* Some platforms return double (android) */
    return Math.trunc(container.scrollTop);
}

function removeBookmarkById(id) {
    var element = document.getElementById(id);
    element.parentNode.removeChild(element);
}

function handleFrameZoom() {
    var frameWindow = window; /*document.getElementById("pageFrame").contentWindow; */
    var frameWindowDocument = frameWindow.document;

    frameWindow.addEventListener('wheel', function (event) {
        if (event.ctrlKey) {
            if (event.wheelDelta > 0) zoomIn();
            else zoomOut();

            event.preventDefault();
        }

    }, { passive: false });
}

function zoomIn() {
    var frameWindow = window; /* document.getElementById("pageFrame").contentWindow; */
    var frameWindowDocument = frameWindow.document;
    var container = frameWindowDocument.getElementById("page-container");

    var zoom = 1.0;
    if (container.style.zoom) {
        zoom = parseFloat(container.style.zoom);
    }

    zoom += 0.1;
    applyZoom(zoom);

    return zoom;
}

function zoomOut() {
    var frameWindow = window; /* document.getElementById("pageFrame").contentWindow; */
    var frameWindowDocument = frameWindow.document;
    var container = frameWindowDocument.getElementById("page-container");

    var zoom = 1.0;
    if (container.style.zoom) {
        zoom = parseFloat(container.style.zoom);
    }

    zoom -= 0.1;
    applyZoom(zoom);

    return zoom;
}

function applyZoom(zoomAmount) {
    if (!zoomAmount) zoomAmount = 1.0;

    var frameWindow = window; /* document.getElementById("pageFrame").contentWindow; */
    var frameWindowDocument = frameWindow.document;
    var container = frameWindowDocument.getElementById("page-container");
    console.log("New Zoom: " + zoomAmount);

    if (container) {
        container.style.zoom = zoomAmount;
    }
}

function removeCurrentSearchMatches() {
    var frameWindow = window; /* document.getElementById("pageFrame").contentWindow; */
    var frameWindowDocument = frameWindow.document;
    let items = frameWindowDocument.querySelectorAll('.ss-search-item');
    
    items.forEach(function (item) {
        item.classList.remove('ss-search-item');
    });
}

function highlightSearchResults(json) {
    try {
        var elementPaths = JSON.parse(json);
        removeCurrentSearchMatches();

        return highlightSearchMatches(elementPaths);
    } catch (e) { console.log(e); }

    return false;
}

function highlightSearchMatches(elementPaths) {
    var isScrolled = false;
    var isComplete = true;

    var frameWindow = window; /* document.getElementById("pageFrame").contentWindow; */
    var frameWindowDocument = frameWindow.document;
	
    elementPaths.forEach(xpath => {
        if (isComplete == false) {
            return false;
        }

        var result = frameWindowDocument
            .evaluate(xpath, frameWindowDocument, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);

        var selectionElement = result.singleNodeValue;
        if (selectionElement == null) {
            isComplete = false;
            return false;
        }

        result = frameWindowDocument
            .evaluate(".//*[@id='page-container']", frameWindowDocument, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);

        var containerElement = result.singleNodeValue;
        if (containerElement == null) {
            isComplete = false;
            return false;
        }

        if (isScrolled === false) {
			selectionElement.scrollIntoView(true);
            isScrolled = true;
        }

        if (selectionElement.classList.contains("ss-search-item")) {
            return true;
        }

        selectionElement.classList.add("ss-search-item");
    });

    return isComplete;
}

function findPosition(node) {
    var top = 0;
    var isExact = true;

    if (node.offsetTop === 0) {
        isExact = false;
    }

    while (node != null) {
        top += (node.offsetTop || 0);
        node = node.parentNode;
    }
	
    return [top, isExact];
}

function getSelectedNodes() {
    var frameWindow = window; /* document.getElementById("pageFrame").contentWindow; */
    var frameWindowDocument = frameWindow.document;
    var selection = frameWindowDocument.getSelection();
	
    if (selection == null || selection.isCollapsed) {
        return;
    }

    var start = selection.anchorNode;
    var end = selection.focusNode;

    if (start == null || end == null) {
        return;
    }

    var position = start.compareDocumentPosition(end);
    if (position & Node.DOCUMENT_POSITION_PRECEDING) {
        [start, end] = [end, start];
    }

    var nodes = new Set();

    while (start != end) {
        if (start.nodeType == 3) {
            nodes.add(start.parentNode);
        }
        else {
            nodes.add(start);
        }

        if (start.firstChild) {
            start = start.firstChild;
        }
        else if (start.nextSibling) {
            start = start.nextSibling;
        }
        else if (start.parentNode.nextSibling) {
            start = start.parentNode.nextSibling;
        } else if (start.parentNode.parentNode.nextSibling) {
            start = start.parentNode.parentNode.nextSibling;
        }
        else if (start.parentNode.parentNode.parentNode.nextSibling) {
            start = start.parentNode.parentNode.parentNode.nextSibling;
        }
        else if (start.parentNode.parentNode.parentNode.parentNode.nextSibling) {
            start = start.parentNode.parentNode.parentNode.parentNode.nextSibling;
        }
        else if (start.parentNode.parentNode.parentNode.parentNode.parentNode.nextSibling) {
            start = start.parentNode.parentNode.parentNode.parentNode.parentNode.nextSibling;
        }
    }

    if (end.nodeType == 3) { /* Text */
        nodes.add(end.parentNode);
    }
    else {
        nodes.add(end);
    }

    var results = Array.from(nodes);
    return results;
}

function getSelectedText() {
    var nodes = getSelectedNodes();
    var text = "";

    if (nodes == null || nodes.length == 0) {
        return "";
    }

    nodes.forEach(node => {
        text += node.innerText;
    });

    return text;
}

function getSelectedXPaths() {
    var nodes = getSelectedNodes();
    var xpath = [];

    if (nodes == null || nodes.length == 0) {
        return [];
    }

    nodes.forEach(node => {
        xpath.push(getPathTo(node));
    });

    return xpath;
}

function getPathTo(element) {
    if (element.id !== '')
        return `id('${element.id}')`;
    if (element === document.body)
        return element.tagName;

    var ix = 0;
    var siblings = element.parentNode.childNodes;

    for (var i = 0; i < siblings.length; i++) {
        var sibling = siblings[i];
        if (sibling === element)
            return `${getPathTo(element.parentNode)}/${element.tagName}[${(ix + 1)}]`;
        if (sibling.nodeType === 1 && sibling.tagName === element.tagName)
            ix++;
    }
}

function updateHighlightColor(xpath, color) {
    var frameWindow = window; /* document.getElementById("pageFrame").contentWindow; */
    var frameWindowDocument = frameWindow.document;

    var result = frameWindowDocument
        .evaluate(xpath, frameWindowDocument, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
    
    if (result == null || result.singleNodeValue == null) {
        alert("Not found");
        return;
    }

    result.singleNodeValue.style = `background-color: ${color};`;
	
	if ((color === null || color.trim() === "") && result.singleNodeValue.classList.contains("ss-search-item")) {
        removeCurrentSearchMatches();
    }
}