document.addEventListener('DOMContentLoaded', function () {

    // Sélectionne tous les éléments ayant une classe contenant "w-action--"
    const elements = document.querySelectorAll('[class*="w-action--"]');

    elements.forEach(function (el) {

        // Trouver la classe qui commence par "w-action--"
        const actionClass = Array.from(el.classList).find(cls => cls.startsWith('w-action--'));

        if (!actionClass) return;

        // Récupérer le nom après "--"
        const actionName = actionClass.split('--')[1];

        // Récupérer le href
        const href = el.getAttribute('href');

        // Ajouter un listener sans bloquer les autres comportements
        el.addEventListener('click', function () {

            fetch('/Home/ContactButton', {
                method: 'POST',
                body: JSON.stringify({
                    clientAction: actionName,
                    targetUrl: href
                }),
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        }, false);
    });
});