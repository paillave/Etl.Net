import { convertToDate } from "./dataAccess";

export const loadState = () => {
    try {
        const serializedState = localStorage.getItem('state');
        if (serializedState === null) {
            return;
        }
        let state = JSON.parse(serializedState);
        convertToDate(state);
        return state;
    }
    catch (err) {
        return;
    }
}

export const saveState = (state) => {
    try {
        const serializedState = JSON.stringify(state);
        localStorage.setItem('state', serializedState);
    }
    catch (err) {

    }
}