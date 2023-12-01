/*
 * Copyright (c) 2020 Raspberry Pi (Trading) Ltd.
 *
 * SPDX-License-Identifier: BSD-3-Clause
 */

 // Ref: https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_watchdog/watchdog.c

#include <stdio.h>

#include "hardware/watchdog.h"
#include "hardware/structs/watchdog.h"
#include "hardware/structs/psm.h"

/// \tag::watchdog_start_tick[]
void watchdog_start_tick(uint cycles);

static uint32_t load_value;

void watchdog_update(void);

uint32_t watchdog_get_count(void);

void _watchdog_enable(uint32_t delay_ms, bool pause_on_debug); 

#define WATCHDOG_NON_REBOOT_MAGIC 0x6ab73121

void watchdog_enable(uint32_t delay_ms, bool pause_on_debug);
void watchdog_reboot(uint32_t pc, uint32_t sp, uint32_t delay_ms);
bool watchdog_caused_reboot(void);
bool watchdog_enable_caused_reboot(void);
