import { from } from "rxjs/internal/observable/from";
import { filter, map, switchMap, withLatestFrom } from "rxjs/operators";
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
            switchMap(i => i.json())
        );
    };
}

function fetchDataApi(restApi, restParameters) {
    const headers = new Headers({
        "Accept": "application/json",
    });

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
            requestInit.headers['content-type'] = 'application/json';
            requestInit.body = JSON.stringify(restParameters.data);
        }
    }

    const url = buildUrl("api", {
        path: apiDescription.path,
        queryParams: restParameters && restParameters.queryParams
    });

    return from(fetch(url, requestInit));
}