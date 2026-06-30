const BOOTSTRAP_ICON_CLASSES = Array.from(document.styleSheets)
    .flatMap(sheet => {
        try {
            return Array.from(sheet.cssRules || []);
        } catch (e) {
            return [];
        }
    })
    .filter(rule => rule.selectorText && rule.selectorText.startsWith('.bi-'))
    .map(rule => 'bi ' + rule.selectorText.split('::')[0].substring(1));


function formatSelect2Username(result) {
    if (result.userId == null) {

        if (firstEmptySelect) {
            console.log('showing row');
            firstEmptySelect = false;
            return '<div class="row">' +
                '<div class="col-xs-2"><b>User Name</b></div>' +
                '<div class="col-xs-2"><b>First Name</b></div>' +
                '<div class="col-xs-2"><b>Last Name</b></div>' +
                '<div class="col-xs-2"><b>Employee ID</b></div>' +
                '<div class="col-xs-2"><b>Email</b></div>' +
                '</div><hr />';


        } else {

            console.log('skipping row');
            return false;
        }
    }

    if (!result.userId) return result.text;


    var markup = "<div><b>" + result.firstName + " " + result.lastName + "</b><div class='pl-1'><small><i>User Name: <b>" + result.userName + "</b>; User Id: <b>" + result.userId + "</b>; Email: <b>" + result.email + "</b></i></small></div></div>";

    return markup;
}

function matchCustom(params, data) {
    firstEmptySelect = true;
    const term = (params && params.term ? params.term : '').trim();

    if (!term) {
        return data;
    }

    var has = true;
    var words = term.toUpperCase().split(" ").filter(Boolean);
    var text = (data && data.text ? data.text : '').toUpperCase();

    for (var i = 0; i < words.length; i++) {
        var word = words[i];
        has = has && (text.indexOf(word) >= 0);
    }

    if (has) return data;
    return false;
}


document.addEventListener('DOMContentLoaded', function () {

    

});