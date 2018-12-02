
// Instantiate an element from a template
// template   - the ID of a <template> element.
// parameters - an object like { labelText: 'field value' }
//              template must have an element with 'data-from' attribute
//              e.g. <p data-from="labelText"></p>
function instantiate(template, parameters) {
    if (typeof (template) === "string") template = $('#' + template);
    var newElement = $(template.html());
    if (typeof (parameters) !== "undefined")
        dataRefresh(newElement, parameters);
    return newElement;
}

function dataRefresh(element, parameters) {
    if (!parameters) return;
    if (typeof (parameters) === "object") {
        if (Array.isArray(parameters)) {
            var dataTemplate = element.attr("data-template");
            var container = element;
            if (!dataTemplate) {
                container = element.find("[data-template]");
                dataTemplate = container.attr('data-template');
            }
            if (!dataTemplate) return;
            container.html("");
            for (var value in parameters) {
                var itemElement = instantiate(dataTemplate, parameters[value]);
                container.append(itemElement);
            }
            return;
        } else for (var key in parameters) {
            var dataValue = parameters[key];
            var target = element.find("[data-from='" + key + "']")
                .addBack("[data-from='" + key + "']");
            if (target.length) {
                if (typeof (dataValue) === "string" || typeof (dataValue) === "number")
                    target.html(dataValue);
                else if (typeof (dataValue) === "object") {
                    dataRefresh(target, dataValue);
                }
                continue;
            }
            var dataFieldTarget = element.find("[data-field='" + key + "']")
                .addBack("[data-field='" + key + "']");
            if (dataFieldTarget.length) {
                dataFieldTarget.data(key, dataValue);
                continue;
            }
            var styleTarget = element.find("[data-css='" + key + "']");
            if (styleTarget.length) {
                for (var cssKey in dataValue) {
                    styleTarget.css(cssKey, dataValue[cssKey]);
                }
                continue;
            }
            var attributeTarget = element.attr('data-attr') === key
                ? element : element.find("[data-attr='" + key + "']");
            if (attributeTarget.length) {
                for (var attrKey in dataValue) {
                    attributeTarget.attr(attrKey, dataValue[attrKey]);
                }
                continue;
            }
        }
    }
    else if (typeof (parameters) === "string") {
        var targetElement = element.find('[data-from]').addBack('[data-from]');
        if (!targetElement.length) return;
        var textFrom = targetElement.attr('data-from');
        if (textFrom === "") targetElement.html(parameters);
    }
}