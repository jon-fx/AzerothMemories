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

function TestImageViewer(id, index) {
    const gallery = new Viewer(document.getElementById(id),
        {
            title: false,
            toolbar: {
                zoomIn: 4,
                zoomOut: 4,
                oneToOne: 4,
                reset: 4,
                prev: 4,
                play: {
                    show: 4,
                    size: 'large',
                },
                next: 4,
                rotateLeft: 0,
                rotateRight: 0,
                flipHorizontal: 0,
                flipVertical: 0,
            },
            initialViewIndex: index,
            movable: false,
            scalable: false,
        });

    gallery.show();
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