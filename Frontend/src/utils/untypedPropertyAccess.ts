export const readProperty = (obj: any, prop: string): any => {
    return obj[prop];
};

export  const writeProperty = (obj: any, prop: string, newValue: any): void => {
    obj[prop] = newValue;
};