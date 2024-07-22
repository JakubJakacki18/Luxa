export function getWindowWidth() {
    return document.documentElement.clientWidth;
}

export function getShortestColumn(columns) {
    let shortestColumn = columns[0];
    for (let i = 1; i < columns.length; i++) {
        if (columns[i].clientHeight < shortestColumn.clientHeight) {
            shortestColumn = columns[i];
        }
    }
    return shortestColumn;
}

export function getOrder(orderDirection) {
    console.log(orderDirection);
    if (orderDirection === "ASC") {
        return true;
    }
    else {
        return false;
    }
};