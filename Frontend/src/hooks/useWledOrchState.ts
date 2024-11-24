import { useEffect, useState } from "react";
import apiClient from "../services/api-client";
import { AxiosError, CanceledError } from "axios";
import { components } from "../types/api";

type LedSegmentGroups = components["schemas"]["DataStoreRoot"][]

export interface WledOchState {
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

type ConditionalProps = GetProps | PutProps

type Props = WledOchState & ConditionalProps

const useWledOrchState = ({method, data}:Props) => {
    const controller = new AbortController()

    const [wledOchState, setWledOchState] = useState<WledOchState>({ ledSegmentGroups: [] });
    const [error, setError] = useState<string>("");
    const [isLoading, setLoading] = useState(false);
    const [hasData, setHasData] = useState(false);

    const useGet = () =>
    {
      setLoading(true);
      setHasData(false);
      apiClient
        .get<LedSegmentGroups>("/state", {signal: controller.signal})
        .then((res) => {
          setWledOchState({ ledSegmentGroups: res.data });
        })
        .catch((err: AxiosError) => {
          if (err instanceof CanceledError) return;
          setError(err.message);
        }).finally(() => {
          setLoading(false);
          setHasData(true);
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
          }).finally(() => {
            setLoading(false);
          });
      }
  
    useEffect(() => {
      if (method == 'GET') {
        useGet();
      } else if (method == 'PUT') {
        usePut(data);
      }
    }, []);

    return {wledOchState, error, isLoading, hasData, apiClient}
}

export default useWledOrchState