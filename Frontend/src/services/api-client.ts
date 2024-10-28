import axios from "axios";

export default axios.create({
    baseURL: import.meta.env.VITE_DEV_BACKEND_ADDRESS ? import.meta.env.VITE_DEV_BACKEND_ADDRESS : window.location.href,
    withCredentials: false,
})