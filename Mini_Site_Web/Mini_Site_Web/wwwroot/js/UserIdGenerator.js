// !!! Demander le  consentement de l'utilisateur avant de stocker son user id !!!

function getCookie(name) {
    return document.cookie
        .split('; ')
        .find(row => row.startsWith(name + '='))
        ?.split('=')[1];
}

function setUserIdCookie() {
    if (getCookie('uid')) return;

    const userId = crypto.randomUUID();

    const oneDay = 60 * 60 * 24;

    document.cookie = [
        `uid=${userId}`,
        `Max-Age=${oneDay}`,
        'Path=/',
        'SameSite=Lax',
        'Secure'
    ].join('; ');
}