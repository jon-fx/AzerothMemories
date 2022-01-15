function ReloadToolTips() {
    $WowheadPower.refreshLinks();
}

function HideWowheadToolTips() {
    $WowheadPower.hideTooltip();
}

function BlazorGetTimeZone() {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
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