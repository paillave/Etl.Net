import { from } from "rxjs/internal/observable/from";
import { filter, map, switchMap, tap } from "rxjs/operators";
import { buildUrl } from "./buildUrl";

function getApiDescription(restApi) {
    if (typeof restApi === "string") {
        return {
            path: restApi,
            method: "GET"
        };
    }
    return restApi;
}

function isFormData(payload) {
    return typeof FormData !== 'undefined' && payload instanceof FormData;
};

export function fetchData(restApi) {
    return function mapOperation(apiCallPayload$) {
        return apiCallPayload$.pipe(
            switchMap(apiParameters => fetchDataApi(restApi, apiParameters)),
            switchMap(i => i.json()),
            tap(i => convertToDate(i))
        );
    };
}

const dateRegex = /^\d{4}-(0?[1-9]|1[012])-(0?[1-9]|[12][0-9]|3[01])/;
function isWindow(obj) { return obj && obj.window === obj; }
function isString(value) { return typeof value === 'string'; }
function isIso8601(value) { return isString(value) && dateRegex.test(value) && !isNaN(Date.parse(value)); }
function isObject(value) { return value !== null && typeof value === 'object'; }
function isFunction(value) { return typeof value === 'function'; }
function isArray(arr) { return Array.isArray(arr) || arr instanceof Array; }
function isNumber(value) { return typeof value === 'number'; }
function isArrayLike(obj) {
    if (obj == null || isWindow(obj)) return false;
    if (isArray(obj) || isString(obj)) return true;
    var length = 'length' in Object(obj) && obj.length;
    return isNumber(length) && (length >= 0 && (length - 1) in obj || typeof obj.item === 'function');
}
function isBlankObject(value) { return value !== null && typeof value === 'object' && !Object.getPrototypeOf(value); }

export function convertToDate(input) {
    if (!isObject(input)) {
        return input;
    }

    forEach(input, (value, key) => {
        if (isIso8601(value)) {
            input[key] = new Date(value);
        } else if (isObject(value)) {
            convertToDate(value);
        }
    });
}
export function formatDate(dt) {
    if (!dt) return '';
    if (!(dt instanceof Date)) return '';
    return `${dt.getFullYear()}-${dt.getMonth()}-${dt.getDate()} ${dt.getHours()}:${dt.getMinutes()}:${dt.getSeconds()}.${dt.getMilliseconds()}`;
}
function forEach(obj, iterator, context) {
    var key, length;
    if (obj) {
        if (isFunction(obj)) {
            for (key in obj) {
                if (key !== 'prototype' && key !== 'length' && key !== 'name' && obj.hasOwnProperty(key)) {
                    iterator.call(context, obj[key], key, obj);
                }
            }
        } else if (isArray(obj) || isArrayLike(obj)) {
            var isPrimitive = typeof obj !== 'object';
            for (key = 0, length = obj.length; key < length; key++) {
                if (isPrimitive || key in obj) {
                    iterator.call(context, obj[key], key, obj);
                }
            }
        } else if (obj.forEach && obj.forEach !== forEach) {
            obj.forEach(iterator, context, obj);
        } else if (isBlankObject(obj)) {
            for (key in obj) {
                iterator.call(context, obj[key], key, obj);
            }
        } else if (typeof obj.hasOwnProperty === 'function') {
            for (key in obj) {
                if (obj.hasOwnProperty(key)) {
                    iterator.call(context, obj[key], key, obj);
                }
            }
        } else {
            for (key in obj) {
                if (hasOwnProperty.call(obj, key)) {
                    iterator.call(context, obj[key], key, obj);
                }
            }
        }
    }
    return obj;
}



function fetchDataApi(restApi, restParameters) {
    const headers = {
        "Accept": "application/json",
    };

    const apiDescription = getApiDescription(restApi);

    const requestInit = {
        headers,
        method: apiDescription.method,
    };

    if (restParameters.data) {
        if (isFormData(restParameters.data)) {
            requestInit.body = restParameters.data;
        }
        else if (requestInit.headers) {
            requestInit.headers['Content-Type'] = 'application/json';
            requestInit.body = JSON.stringify(restParameters.data);
        }
    }

    const url = buildUrl("api", {
        path: apiDescription.path,
        queryParams: restParameters && restParameters.queryParams
    });

    return from(fetch(url, requestInit));
}