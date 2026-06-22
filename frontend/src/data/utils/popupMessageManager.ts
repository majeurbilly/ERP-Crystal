import { toast } from 'react-toastify';

export const notifyMessage = (text: string = "Wow! so easy!") => toast(text);
export const notifySuccessMessage = (successText: string) => toast.success(successText);
export const notifyErrorMessage = (errorText: string) => toast.error(errorText);