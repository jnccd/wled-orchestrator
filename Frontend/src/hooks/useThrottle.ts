import { useRef } from "react";

const useThrottle = () => {
  const throttleSeed = useRef(null as NodeJS.Timeout | null);

  const throttleFunction = useRef((func: () => void, delay: number = 200) => {
    if (!throttleSeed.current) {
      // Call the callback immediately for the first time
      func();
      throttleSeed.current = setTimeout(() => {
        throttleSeed.current = null;
      }, delay);
    }
  });

  return throttleFunction.current;
};

export default useThrottle;