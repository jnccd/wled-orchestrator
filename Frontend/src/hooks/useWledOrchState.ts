import { useEffect, useState } from "react";
import apiClient from "../services/api-client";
import { AxiosError, CanceledError } from "axios";
import { components } from "../types/api";

type LedSegmentGroups = components["schemas"]["DataStoreRoot"]

export interface WledOrchState {
    ledSegmentGroups: LedSegmentGroups,
}

type GetProps = {
  method:"GET",
  data?: never
}
type PutProps = {
  method:"PUT",
  data: LedSegmentGroups
}

type Props =GetProps | PutProps

const useWledOrchState = ({method, data}: Props = {
  method: "GET",
  data: undefined,
}) => {
    const controller = new AbortController()

    const [wledOrchState, setWledOrchState] = useState<WledOrchState>({ ledSegmentGroups: {} });
    const [error, setError] = useState<string>("");
    const [isLoading, setLoading] = useState(false);
    const [hasData, setHasData] = useState(false);
    const [effectTrigger, setEffectTrigger] = useState(false);

    const useGet = () =>
    {
      setLoading(true);
      setHasData(false);
      apiClient
        .get<LedSegmentGroups>("/state", {signal: controller.signal})
        .then((res) => {
          setWledOrchState({ ledSegmentGroups: res.data });
          setHasData(true);
        })
        .catch((err: AxiosError) => {
          if (err instanceof CanceledError) return;
          setError(err.message);
        })
        .finally(() => {
          setLoading(false);
        });
    }

    const usePut = (data: LedSegmentGroups) =>
    {
      setLoading(true);
      apiClient
        .put<LedSegmentGroups>("/state", data, {signal: controller.signal})
        .then((_) => { })
        .catch((err: AxiosError) => {
          if (err instanceof CanceledError) return;
          setError(err.message);
        })
        .finally(() => {
          setLoading(false);
        });
    }
  
    useEffect(() => {
      if (method == 'GET') {
        useGet();
      } else if (method == 'PUT') {
        usePut(data);
      }
    }, [effectTrigger]);

    const refresh = () => {
      setEffectTrigger(!effectTrigger);
    };

    return {wledOrchState, error, isLoading, hasData, refresh}
}

export default useWledOrchState