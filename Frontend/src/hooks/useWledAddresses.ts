import { useEffect, useState } from "react";
import apiClient from "../services/api-client";
import { AxiosError, CanceledError } from "axios";

export interface WledAddresses {
    addresses: [string],
}

const useWledAddresses = () => {
    const controller = new AbortController()

    const [wledAddresses, setWledAddresses] = useState<WledAddresses>({ addresses: [""] });
    const [error, setError] = useState<string>("");
    const [isLoading, setLoading] = useState(false);
  
    useEffect(() => {
      setLoading(true)
      apiClient
        .get<[string]>("/wledServers", {signal: controller.signal})
        .then((res) => {
          setWledAddresses({ addresses: res.data })
          setLoading(false)
        })
        .catch((err: AxiosError) => {
            if (err instanceof CanceledError) return;
          setError(err.message);
          setLoading(false)
        });

        return () => {
        }
    }, []);

    return {wledAddresses, error, isLoading, apiClient}
}

export default useWledAddresses