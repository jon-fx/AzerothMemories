function ReloadToolTips() {
    $WowheadPower.refreshLinks();
}

function HideWowheadToolTips() {
    $WowheadPower.hideTooltip();
}

function BlazorGetTimeZone() {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
}

function SetImage(id, type, data) {
    document.getElementById(id).src = URL.createObjectURL(
        new Blob([data], { type: type })
    );
}

function GetAntiForgeryToken() {
    var elements = document.getElementsByName('__RequestVerificationToken');
    if (elements.length > 0) {
        return elements[0].value;
    }

    console.warn('no anti forgery token found!');
    return null;
}

function OpenImageEditor(id, dotNetHelper) {
    Painterro({
        toolbarHeightPx: 54,
        buttonSizePx: 42,
        backgroundFillColor: '#222',

        hideByEsc: true,
        saveByEnter: true,

        hiddenTools: [/*'line', 'arrow', 'rect',*/ 'ellipse', 'brush', /*'text',*/ 'rotate', 'resize', /*'save'*/, 'open',/* 'close'*/, /*'undo', 'redo',*/ 'bucket'],
        pixelizePixelSize: 10,
        pixelizeHideUserInput: true,

        saveHandler: function (image, done) {
            document.getElementById(id).src = image.asDataURL('image/jpeg', 1);

            //console.log(image.getWidth());
            //console.log(image.getHeight());

            image.asBlob('image/jpeg', 1).arrayBuffer().then(buffer => {
                dotNetHelper.invokeMethodAsync('UpdateImage', new Uint8Array(buffer));
                done(true);
            });
        }
    }).show(document.getElementById(id).src);
}

function InitializeImageViewer(index, gallery) {
    Spotlight.show(gallery, {
        index: index + 1,
    });
}

function SetUpTagTextBox(textBoxName, userTags) {
    if (window.hasOwnProperty('mainTribute') && window.hasOwnProperty('mainTributeAttached')) {
        window.mainTribute.detach(window.mainTributeAttached);
    }

    window.mainTribute = new Tribute({
        collection: [
            {
                lookup: 'key',
                fillAttr: 'key',
                values: userTags,
                spaceSelectsMatch: true,
                containerClass: 'mud-popover mud-popover-open mud-paper mud-elevation-8',
                itemClass: 'mud-list-item mud-list-item-dense mud-list-item-gutters mud-list-item-clickable mud-ripple',
                selectClass: 'mud-selected-item'
            }],
        spaceSelectsMatch: true
    });

    window.mainTributeAttached = document.getElementById(textBoxName);
    window.mainTribute.attach(window.mainTributeAttached);
}