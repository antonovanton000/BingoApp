jQuery(function ($) {
    InitI18N();
});

var curLocale = "";
function InitI18N() {
    curLocale = $.cookie('locale');
    if (curLocale == '' || curLocale == undefined) {
        var curUserLocale = navigator.language;
        if (curUserLocale.toLowerCase().includes('ru')) {
            curLocale = 'ru';
        }
        else if (curUserLocale.toLowerCase().includes('en')) {
            curLocale = 'en';
        }
        else {
            curLocale = 'en';
        }
    }

    var el = $('.display-locale');
    el.html(curLocale);
    el.prop('data-locale', curLocale);
    $('a.switch-locale[data-locale="' + curLocale + '"]').parent().addClass('d-none');

    console.log(curLocale);
    $.i18n({
        locale: curLocale,
        useLocalStorage: true,
        useDataAttrOptions: true
    }).load({
        'en': '/lib/jquery.i18n/translations/en.json?v=' + (new Date()).getTime(),
        'ru': '/lib/jquery.i18n/translations/ru.json?v=' + (new Date()).getTime(),
    })
        .done(function () {
            $('a.switch-locale').on('click', function (e) {
                e.preventDefault();
                var locale = $(this).data('locale');
                $.i18n().locale = locale;
                $.cookie('locale', locale, { path: '/', expires: 365 });
                window.location.reload();
            });
            if (curLocale != "ru") {
                $('html').i18n();
                $('.translate').each(function () {
                    var args = [], $this = $(this);
                    if ($this.data('args'))
                        args = $this.data('args').split(',');
                    $this.html($.i18n.apply(null, args));
                });
                $('[data-bs-toggle="tooltip"]').tooltip();
            }
        });
}

function TranslateValidation() {
    if (curLocale == 'en') {

        jQuery.extend(jQuery.validator.messages, {
            required: "This field is required.",
            remote: "Please fix this field.",
            email: "Please enter a valid email address.",
            url: "Please enter a valid URL.",
            date: "Please enter a valid date.",
            dateISO: "Please enter a valid date (ISO).",
            number: "Please enter a valid number.",
            digits: "Please enter only digits.",
            creditcard: "Please enter a valid credit card number.",
            equalTo: "Please enter the same value again.",
            accept: "Please enter a value with a valid extension.",
            maxlength: jQuery.validator.format("Please enter no more than {0} characters."),
            minlength: jQuery.validator.format("Please enter at least {0} characters."),
            rangelength: jQuery.validator.format("Please enter a value between {0} and {1} characters long."),
            range: jQuery.validator.format("Please enter a value between {0} and {1}."),
            max: jQuery.validator.format("Please enter a value less than or equal to {0}."),
            min: jQuery.validator.format("Please enter a value greater than or equal to {0}.")
        });
    }
    if (curLocale == 'ru') {
        jQuery.extend(jQuery.validator.messages, {
            required: "Это поле обязательно для заполнения.",
            remote: "Пожалуйста, исправьте это поле",
            email: "Пожалуйста, введите действительный адрес электронной почты.",
            url: "Пожалуйста, введите действительный URL.",
            date: "Пожалуйста, введите правильную дату.",
            dateISO: "Пожалуйста, введите правильную дату(ISO).",
            number: "Пожалуйста, введите действительное число",
            digits: "Пожалуйста, введите только цифры.",
            creditcard: "Пожалуйста, введите действительный номер кредитной карты.",
            equalTo: "Пожалуйста, введите то же значение еще раз.",
            accept: "Пожалуйста, введите значение с допустимым расширением.",
            maxlength: jQuery.validator.format("Введите не более {0} символов."),
            minlength: jQuery.validator.format("Введите не менее {0} символов."),
            rangelength: jQuery.validator.format("Пожалуйста, введите значение от {0} до {1} символов."),
            range: jQuery.validator.format("Введите значение от {0} до {1}."),
            max: jQuery.validator.format("Пожалуйста, введите значение, меньшее или равное {0}."),
            min: jQuery.validator.format("Пожалуйста, введите значение больше или равное {0}.")
        });
    }
}

function viewPassword(el) {
    el = $(el);
    var input = el.parent().find('input');
    if (input.attr('type') == 'password') {
        input.attr('type', 'text')
    } else {
        input.attr('type', 'password')
    }
}

function getURLParameter(name) {
    var res = decodeURIComponent((new RegExp('[?|&]' + name + '=' + '([^&;]+?)(&|#|;|$)').exec(location.search) || [, ""])[1].replace(/\+/g, '%20')) || null;
    if (res == null) {
        res = location.search.indexOf(name + "=") > -1 ? "" : null;
    }
    return res;
}

function changeParam(url, urlparam) {
    var currentURL = url + ((url.indexOf('?') > 0) ? '&' : '');
    urlparam = urlparam.replace('?', '');

    var params = urlparam.split('&');
    var newURL = currentURL;
    for (var i in params) {
        if (params[i] != "") {
            var param = params[i].substring(0, params[i].indexOf('='));
            var value = params[i].substring(params[i].indexOf('=') + 1);
            var change = new RegExp('(' + param + ')=(.*)&', 'g');
            if (currentURL.indexOf(param) > 0) {
                newURL = currentURL.replace(change, '$1=' + value + '&');
                newURL = newURL + currentURL.substr(currentURL.indexOf("&", currentURL.indexOf(param, currentURL.indexOf("?") + 1) + 1) + 1);
            }
            else {
                newURL += ((currentURL.indexOf('?') > 0) ? '' : '?') + param + '=' + value + "&";
            }
        }
        currentURL = newURL;
    }
    return newURL.substring(0, newURL.length - 1);
}

function changeUrlParam(param, value) {
    var currentURL = window.location.href + '&';
    var change = new RegExp('(' + param + ')=(.*)&', 'g');
    var newURL = currentURL.replace(change, '$1=' + value + '&');
    newURL = newURL + currentURL.substr(currentURL.indexOf("&", currentURL.indexOf(param, currentURL.indexOf("?") + 1) + 1) + 1);

    if (getURLParameter(param) !== null) {
        try {
            window.history.replaceState('', '', newURL.slice(0, -1));
        } catch (e) {
            console.log(e);
        }
    } else {
        var currURL = window.location.href;
        if (currURL.indexOf("?") !== -1) {
            window.history.replaceState('', '', currentURL.slice(0, -1) + '&' + param + '=' + value);
        } else {
            window.history.replaceState('', '', currentURL.slice(0, -1) + '?' + param + '=' + value);
        }
    }
}

function getI18nKeys() {
    var res = {};
    $('[data-i18n]').each(function (index, item) {
        var $item = $(item);
        var key = $item.data('i18n').replace('[html]', '').replace('[title]', '').replace('[placeholder]', '');
        res[key] = $item.html().replace('\n', '').replace('\r', '').replace('\t', '').trim();
    });

    return res;
}