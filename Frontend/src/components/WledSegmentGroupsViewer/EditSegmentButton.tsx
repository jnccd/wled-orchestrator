import EditButton from "../EditButton";
import {
  Button,
  FormLabel,
  Input,
  NumberDecrementStepper,
  NumberInput,
  NumberInputField,
  NumberInputStepper,
  NumberIncrementStepper,
  Text,
  VStack,
} from "@chakra-ui/react";
import React, { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  LedSegment,
  renameSegment,
  setSegmentGammaExponentOverride,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";

interface Props {
  segment: LedSegment;
}

// Upper bound for the per-segment gamma override (0 = use the devices own reported gamma).
const maxGammaExponentOverride = 10;

const EditSegmentButton = ({ segment }: Props) => {
  const inputId = "name-input-" + segment.id;
  const gammaInputId = "gamma-input-" + segment.id;
  // The server's own reported gamma, used as the baseline reference value in the input.
  const deviceGamma = segment.deviceGammaExponent ?? 0;
  // Whether the user already set an explicit override (vs. using the device default).
  const hasOverride = (segment.gammaExponentOverride ?? 0) > 0;
  // Pre-fill with the current effective gamma (override if set, otherwise the device gamma) so the
  // user sees the value actually in use instead of a bare 0.
  const [gammaValue, setGammaValue] = useState<number>(
    hasOverride ? (segment.gammaExponentOverride ?? 0) : deviceGamma,
  );

  // React Query setup
  const queryClient = useQueryClient();
  const renameSegmentMutation = useMutation({
    mutationFn: renameSegment,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });
  const setGammaMutation = useMutation({
    mutationFn: setSegmentGammaExponentOverride,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });

  const submit = (onClose: () => void) => {
    if (!segment || !segment.id) {
      console.log("segment null??");
      return;
    }
    const newName =
      (document.getElementById(inputId) as HTMLInputElement)?.value ?? "";

    let gamma = 0;
    if (Number.isFinite(gammaValue)) {
      const clamped = Math.min(
        Math.max(gammaValue, 0),
        maxGammaExponentOverride,
      );
      // If there was no explicit override and the user left the value at the device baseline, send 0
      // so the server keeps "use device default" instead of writing a redundant override.
      gamma =
        !hasOverride && Math.abs(clamped - deviceGamma) < 0.0001 ? 0 : clamped;
    }

    renameSegmentMutation.mutate({ segmentId: segment.id, newName });
    setGammaMutation.mutate({
      segmentId: segment.id,
      gammaExponentOverride: gamma,
    });

    onClose();
  };

  return (
    <EditButton
      children={(_a, _b, onClose, firstFieldRef) => {
        return (
          <VStack alignItems={"left"}>
            <FormLabel
              textAlign={"left"}
              htmlFor={inputId}
              marginTop={4}
              marginBottom={1}
            >
              Name:
            </FormLabel>
            <Input
              id={inputId}
              ref={firstFieldRef}
              defaultValue={segment.name ?? ""}
              onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                if (e.key === "Enter") submit(onClose);
              }}
            />
            <FormLabel
              textAlign={"left"}
              htmlFor={gammaInputId}
              marginTop={3}
              marginBottom={0}
            >
              Gamma Exponent:
            </FormLabel>
            <NumberInput
              id={gammaInputId}
              min={0}
              max={maxGammaExponentOverride}
              step={0.1}
              value={gammaValue}
              onChange={(_valueString, valueNumber) =>
                setGammaValue(Number.isNaN(valueNumber) ? 0 : valueNumber)
              }
            >
              <NumberInputField
                onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                  if (e.key === "Enter") submit(onClose);
                }}
              />
              <NumberInputStepper>
                <NumberIncrementStepper />
                <NumberDecrementStepper />
              </NumberInputStepper>
            </NumberInput>
            <Text fontSize="xs" color="gray.500" textAlign="left">
              Device default: {deviceGamma.toFixed(2)}
              {hasOverride ? " (overridden)" : ""}
            </Text>
            <Button onClick={() => submit(onClose)}>Submit</Button>
          </VStack>
        );
      }}
    ></EditButton>
  );
};

export default EditSegmentButton;
